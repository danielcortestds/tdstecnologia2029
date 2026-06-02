using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

public interface IEmailSender
{
    Task EnviarContatoAsync(ContatoForm contato);
}

/// <summary>
/// Envia o e-mail do formulário de contato via SMTP usando MailKit.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<SmtpSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnviarContatoAsync(ContatoForm contato)
    {
        if (!_settings.EstaConfigurado)
        {
            _logger.LogError("[TDS] SMTP não configurado. Defina as configurações Smtp__* no Azure.");
            throw new InvalidOperationException("O serviço de e-mail não está configurado.");
        }

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
        mensagem.To.Add(MailboxAddress.Parse(_settings.To));
        // Permite responder direto ao visitante que enviou o contato.
        mensagem.ReplyTo.Add(new MailboxAddress(contato.Nome, contato.Email));
        mensagem.Subject = $"Novo contato pelo site — {contato.Nome}";

        var corpo = $"""
            <h2>Novo contato pelo site TDS Tecnologia</h2>
            <p><strong>Nome:</strong> {Escapar(contato.Nome)}</p>
            <p><strong>E-mail:</strong> {Escapar(contato.Email)}</p>
            <p><strong>Telefone:</strong> {Escapar(contato.Telefone)}</p>
            <p><strong>Mensagem:</strong></p>
            <p>{Escapar(contato.Mensagem).Replace("\n", "<br>")}</p>
            """;

        mensagem.Body = new BodyBuilder
        {
            HtmlBody = corpo,
            TextBody = $"Nome: {contato.Nome}\nE-mail: {contato.Email}\nTelefone: {contato.Telefone}\n\n{contato.Mensagem}"
        }.ToMessageBody();

        using var client = new SmtpClient();

        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect      // porta 465
            : SecureSocketOptions.StartTls;         // porta 587

        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);

        if (!string.IsNullOrWhiteSpace(_settings.User))
            await client.AuthenticateAsync(_settings.User, _settings.Password);

        await client.SendAsync(mensagem);
        await client.DisconnectAsync(true);

        _logger.LogInformation("[TDS] E-mail de contato enviado de {Email}.", contato.Email);
    }

    private static string Escapar(string texto) =>
        System.Net.WebUtility.HtmlEncode(texto ?? string.Empty);
}
