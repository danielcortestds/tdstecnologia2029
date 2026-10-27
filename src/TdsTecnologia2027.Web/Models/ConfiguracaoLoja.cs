using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Credenciais da loja / InfinitePay. Uma única linha, editável em
/// Admin → Loja. O segredo fica no banco (não em código), como a de e-mail.
/// </summary>
public class ConfiguracaoLoja
{
    public int Id { get; set; }

    /// <summary>InfiniteTag da conta (sem o "$"). Identifica o recebedor.</summary>
    [StringLength(80)]
    [Display(Name = "InfiniteTag (sem o $)")]
    public string? Handle { get; set; }

    /// <summary>API Key do Checkout (gerada no app InfinitePay). Opcional.</summary>
    [StringLength(120)]
    [Display(Name = "API Key do Checkout")]
    public string? ApiKey { get; set; }

    [Display(Name = "Pagamento online ativo")]
    public bool Ativo { get; set; }

    /// <summary>True quando dá para gerar um link de pagamento.</summary>
    public bool EstaConfigurado => Ativo && !string.IsNullOrWhiteSpace(Handle);
}
