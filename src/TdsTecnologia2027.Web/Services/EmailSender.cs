using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

public interface IEmailSender
{
    /// <summary>Envia o e-mail do formulário de contato para o destino configurado.</summary>
    Task EnviarContatoAsync(ContatoForm contato);

    /// <summary>Envio genérico (usado pelo botão "enviar e-mail de teste" do admin).</summary>
    Task EnviarAsync(string para, string assunto, string corpoHtml, string? responderPara = null);
}

/// <summary>
/// Envia e-mail via Microsoft Graph (Office 365) usando o fluxo de credenciais de
/// aplicativo (client credentials). As credenciais ficam na tabela ConfiguracaoEmail,
/// editável pela área administrativa. Não depende de SMTP, senha de usuário nem MFA.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ApplicationDbContext db, ILogger<EmailSender> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EnviarContatoAsync(ContatoForm contato)
    {
        var assunto = $"Novo contato pelo site — {contato.Nome}";
        var corpo = $"""
            <h2>Novo contato pelo site TDS Tecnologia</h2>
            <p><strong>Nome:</strong> {Escapar(contato.Nome)}</p>
            <p><strong>E-mail:</strong> {Escapar(contato.Email)}</p>
            <p><strong>Telefone:</strong> {Escapar(contato.Telefone)}</p>
            <p><strong>Mensagem:</strong></p>
            <p>{Escapar(contato.Mensagem).Replace("\n", "<br>")}</p>
            """;

        // Permite responder diretamente ao visitante que enviou o contato.
        await EnviarAsync(para: null!, assunto: assunto, corpoHtml: corpo, responderPara: contato.Email);
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml, string? responderPara = null)
    {
        var cfg = await _db.ConfiguracoesEmail.AsNoTracking().FirstOrDefaultAsync();

        if (cfg is null || !cfg.Ativo)
            throw new InvalidOperationException(
                "O envio de e-mail (Office 365) não está ativado. Configure em Admin → Configurações.");

        if (!cfg.EstaConfigurado)
            throw new InvalidOperationException(
                "Configuração de e-mail incompleta. Preencha Client ID, Tenant ID, Secret, remetente e destino.");

        // Se não foi passado destinatário explícito, usa o destino configurado (formulário de contato).
        var destino = string.IsNullOrWhiteSpace(para) ? cfg.EmailDestino! : para;

        var credential = new ClientSecretCredential(cfg.TenantId, cfg.ClientId, cfg.ClientSecret);
        var graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

        var mensagem = new Message
        {
            Subject = assunto,
            Body = new ItemBody { ContentType = BodyType.Html, Content = corpoHtml },
            ToRecipients = new List<Recipient>
            {
                new() { EmailAddress = new EmailAddress { Address = destino } }
            }
        };

        if (!string.IsNullOrWhiteSpace(responderPara))
        {
            mensagem.ReplyTo = new List<Recipient>
            {
                new() { EmailAddress = new EmailAddress { Address = responderPara } }
            };
        }

        await graph.Users[cfg.EmailRemetente]
            .SendMail
            .PostAsync(new SendMailPostRequestBody { Message = mensagem, SaveToSentItems = true });

        _logger.LogInformation("[TDS] E-mail enviado via Graph para {Destino}.", destino);
    }

    private static string Escapar(string texto) =>
        System.Net.WebUtility.HtmlEncode(texto ?? string.Empty);
}
