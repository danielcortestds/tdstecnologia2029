using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Dados enviados pelo formulário "Entre em Contato" da página inicial.
/// </summary>
public class ContatoForm
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu telefone.")]
    [StringLength(40)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(2000)]
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Campo "armadilha" (honeypot) invisível para humanos. Se vier preenchido,
    /// é um robô de spam e a mensagem é descartada silenciosamente.
    /// </summary>
    public string? Website { get; set; }
}
