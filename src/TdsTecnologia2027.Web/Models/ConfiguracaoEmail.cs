using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Configuração do envio de e-mail via Microsoft Graph (Office 365), editável
/// pela área administrativa. Guarda uma única linha (Id = 1).
/// </summary>
public class ConfiguracaoEmail
{
    public int Id { get; set; }

    [Display(Name = "Client ID")]
    [StringLength(100)]
    public string? ClientId { get; set; }

    [Display(Name = "Tenant ID")]
    [StringLength(100)]
    public string? TenantId { get; set; }

    [Display(Name = "Client Secret")]
    [StringLength(300)]
    public string? ClientSecret { get; set; }

    [Display(Name = "E-mail remetente")]
    [EmailAddress(ErrorMessage = "E-mail remetente inválido.")]
    [StringLength(150)]
    public string? EmailRemetente { get; set; }

    [Display(Name = "Receber contatos em")]
    [EmailAddress(ErrorMessage = "E-mail de destino inválido.")]
    [StringLength(150)]
    public string? EmailDestino { get; set; }

    [Display(Name = "Ativar envio de e-mail")]
    public bool Ativo { get; set; }

    /// <summary>True quando há configuração mínima para enviar.</summary>
    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(TenantId) &&
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        !string.IsNullOrWhiteSpace(EmailRemetente) &&
        !string.IsNullOrWhiteSpace(EmailDestino);
}
