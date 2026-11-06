using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

public enum StatusAluno
{
    Ativo = 0,
    Inativo = 1,
    Bloqueado = 2
}

/// <summary>
/// Aluno (Área do Aluno). A partir de nov/2026 os alunos têm autenticação PRÓPRIA,
/// totalmente separada da administração: login/senha ficam NESTA tabela (SenhaHash),
/// e não mais no ASP.NET Identity (AspNetUsers), que agora é só dos administradores.
/// </summary>
public class Aluno
{
    public int Id { get; set; }

    /// <summary>
    /// (Legado) Id do antigo IdentityUser. Mantido só para a migração dos dados;
    /// não é mais usado na autenticação.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>Hash da senha do aluno (PasswordHasher). Null = ainda sem senha definida.</summary>
    public string? SenhaHash { get; set; }

    /// <summary>Token de recuperação de senha (uso único) e sua validade.</summary>
    public string? TokenReset { get; set; }
    public DateTime? TokenResetExpira { get; set; }

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
