using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Certificado de conclusão emitido quando o aluno atinge o mínimo de progresso
/// no curso. Guarda snapshots (nome/curso/carga) para o certificado continuar
/// válido mesmo que o curso mude depois.
/// </summary>
public class Certificado
{
    /// <summary>Percentual mínimo de conclusão para emitir o certificado.</summary>
    public const int PercentualMinimo = 65;

    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    /// <summary>Código único usado na URL de validação (e no QR).</summary>
    [Required]
    [StringLength(20)]
    public string CodigoValidacao { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string NomeAluno { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string CursoNome { get; set; } = string.Empty;

    public int CargaHoraria { get; set; }

    /// <summary>Mês/período em que o aluno realizou o curso (ex.: "Outubro de 2026").</summary>
    [StringLength(80)]
    public string? PeriodoExecucao { get; set; }

    public int PercentualNaEmissao { get; set; }

    public DateTime EmitidoEm { get; set; } = DateTime.Now;
}
