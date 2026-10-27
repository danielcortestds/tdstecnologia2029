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

    /// <summary>E-mail de liberação de acesso (compra ou voucher), com login e senha se for conta nova.</summary>
    Task EnviarAcessoAsync(string email, string nome, IEnumerable<string> cursos, string? senhaTemporaria);
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
    private readonly IHttpContextAccessor _http;

    public EmailSender(ApplicationDbContext db, ILogger<EmailSender> logger, IHttpContextAccessor http)
    {
        _db = db;
        _logger = logger;
        _http = http;
    }

    public async Task EnviarAcessoAsync(string email, string nome, IEnumerable<string> cursos, string? senhaTemporaria)
    {
        var req = _http.HttpContext?.Request;
        var baseUrl = req != null ? $"{req.Scheme}://{req.Host}" : "https://tdstecnologia.com.br";
        var loginUrl = $"{baseUrl}/Aluno";

        var listaCursos = string.Join("", cursos.Select(c => $"<li>{Escapar(c)}</li>"));

        var blocoAcesso = string.IsNullOrWhiteSpace(senhaTemporaria)
            ? $"<p>Use o e-mail e a senha que você já tem na plataforma. Esqueceu a senha? Fale com a gente.</p>"
            : $"""
                <p>Criamos seu acesso. Use os dados abaixo para entrar (recomendamos trocar a senha depois):</p>
                <table style="border-collapse:collapse; margin:12px 0;">
                  <tr><td style="padding:6px 14px; background:#f1f5f9; font-weight:bold;">E-mail</td><td style="padding:6px 14px; border:1px solid #e2e8f0;">{Escapar(email)}</td></tr>
                  <tr><td style="padding:6px 14px; background:#f1f5f9; font-weight:bold;">Senha</td><td style="padding:6px 14px; border:1px solid #e2e8f0;"><code>{Escapar(senhaTemporaria!)}</code></td></tr>
                </table>
                """;

        var corpo = $"""
            <div style="font-family:Arial,sans-serif; color:#0f172a;">
              <h2 style="color:#0d9488;">Acesso liberado! 🎉</h2>
              <p>Olá, {Escapar(nome)}!</p>
              <p>Seu acesso ao(s) curso(s) abaixo foi liberado na Área do Aluno da TDS Tecnologia:</p>
              <ul>{listaCursos}</ul>
              {blocoAcesso}
              <p style="margin:24px 0;">
                <a href="{loginUrl}" style="background:#14b8a6; color:#04201c; font-weight:bold; text-decoration:none; padding:12px 26px; border-radius:999px; display:inline-block;">Acessar meus cursos</a>
              </p>
              <p style="color:#64748b; font-size:13px;">Se o botão não funcionar, acesse: {loginUrl}</p>
              <p style="color:#64748b; font-size:13px;">Bons estudos! — Equipe TDS Tecnologia</p>
            </div>
            """;

        await EnviarAsync(para: email, assunto: "Seu acesso na TDS Tecnologia está liberado", corpoHtml: corpo);
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
