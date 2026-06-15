using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

public class AlunoListaItem
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Empresa { get; set; }
    public StatusAluno Status { get; set; }
    public int CursosLiberados { get; set; }
}

public class NovoAlunoViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [StringLength(40)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(150)]
    [Display(Name = "Empresa")]
    public string? Empresa { get; set; }

    [Display(Name = "Status")]
    public StatusAluno Status { get; set; } = StatusAluno.Ativo;
}

/// <summary>Item de liberação de curso na tela de edição do aluno.</summary>
public class AcessoCursoItem
{
    public int CursoOnlineId { get; set; }
    public string CursoNome { get; set; } = string.Empty;
    public bool Liberado { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DataInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DataFim { get; set; }
}

public class EditarAlunoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [StringLength(40)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(150)]
    [Display(Name = "Empresa")]
    public string? Empresa { get; set; }

    [Display(Name = "Status")]
    public StatusAluno Status { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha (opcional)")]
    public string? NovaSenha { get; set; }

    /// <summary>Lista de todos os cursos online com o estado de liberação para este aluno.</summary>
    public List<AcessoCursoItem> Acessos { get; set; } = new();
}
