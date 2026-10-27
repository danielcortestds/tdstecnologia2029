using System.ComponentModel.DataAnnotations;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu telefone/WhatsApp.")]
    [StringLength(40)]
    [Display(Name = "Telefone / WhatsApp")]
    public string Telefone { get; set; } = string.Empty;

    public List<ItemCarrinho> Itens { get; set; } = new();
    public decimal Total { get; set; }
}

public class ResgateViewModel
{
    [Required(ErrorMessage = "Informe o código do voucher.")]
    [StringLength(30)]
    [Display(Name = "Código do voucher")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
}
