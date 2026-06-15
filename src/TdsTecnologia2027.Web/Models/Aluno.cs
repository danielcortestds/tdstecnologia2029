using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

public enum StatusAluno
{
    Ativo = 0,
    Inativo = 1,
    Bloqueado = 2
}

/// <summary>
/// Perfil do aluno (Área do Aluno). A autenticação reutiliza o ASP.NET Identity:
/// cada Aluno está ligado a um IdentityUser (UserId) com o papel "Aluno".
/// </summary>
public class Aluno
{
    public int Id { get; set; }

    /// <summary>Id do IdentityUser correspondente (AspNetUsers).</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(150)]
    [Display(Name = "Empresa")]
    public string? Empresa { get; set; }

    [Display(Name = "Status")]
    public StatusAluno Status { get; set; } = StatusAluno.Ativo;

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<MatriculaCurso> Matriculas { get; set; } = new List<MatriculaCurso>();
}
