namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Configurações do servidor de e-mail (SMTP). Os valores reais são definidos
/// nas "Configurações de aplicativo" do Azure (nunca no código), usando as chaves:
///   Smtp__Host, Smtp__Port, Smtp__User, Smtp__Password, Smtp__From, Smtp__FromName,
///   Smtp__To, Smtp__UseSsl
/// </summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    /// <summary>Servidor SMTP, ex: mail.tdstecnologia.com.br</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Porta: 587 (STARTTLS) ou 465 (SSL).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Usuário de autenticação (normalmente o e-mail completo).</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>Senha da conta de e-mail.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>E-mail remetente (de onde a mensagem sai).</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Nome exibido como remetente.</summary>
    public string FromName { get; set; } = "Site TDS Tecnologia";

    /// <summary>E-mail que vai RECEBER os contatos do site.</summary>
    public string To { get; set; } = string.Empty;

    /// <summary>true = conexão SSL direta (porta 465); false = STARTTLS (porta 587).</summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>True quando há configuração mínima para enviar e-mail.</summary>
    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(From) &&
        !string.IsNullOrWhiteSpace(To);
}
