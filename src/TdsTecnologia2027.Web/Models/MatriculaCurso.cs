using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Liberação de acesso de um aluno a um curso online, com janela de datas.
/// </summary>
public class MatriculaCurso
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    [Display(Name = "Início do acesso")]
    [DataType(DataType.Date)]
    public DateTime? DataInicio { get; set; }

    [Display(Name = "Fim do acesso")]
    [DataType(DataType.Date)]
    public DateTime? DataFim { get; set; }

    /// <summary>Acesso liberado (true) ou removido (false).</summary>
    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    /// <summary>True se o acesso está liberado e dentro da janela de datas (hoje).</summary>
    public bool AcessoValido()
    {
        if (!Ativo) return false;
        var hoje = DateTime.Today;
        if (DataInicio.HasValue && hoje < DataInicio.Value.Date) return false;
        if (DataFim.HasValue && hoje > DataFim.Value.Date) return false;
        return true;
    }
}
