using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Situação de uma assinatura.</summary>
public enum StatusAssinatura
{
    Ativa = 0,
    Expirada = 1,
    Cancelada = 2
}

/// <summary>
/// Assinatura de um aluno a um <see cref="PlanoAssinatura"/> (ex.: TDS Pass).
/// Enquanto ativa e dentro da janela de datas, libera o acesso a TODOS os cursos
/// online publicados — sem precisar de matrícula por curso.
/// </summary>
public class Assinatura
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int PlanoAssinaturaId { get; set; }
    public PlanoAssinatura? Plano { get; set; }

    [Display(Name = "Início")]
    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [Display(Name = "Fim do acesso")]
    [DataType(DataType.Date)]
    public DateTime DataFim { get; set; }

    [Display(Name = "Situação")]
    public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;

    /// <summary>Pedido que originou a assinatura (null para liberação manual).</summary>
    public int? PedidoId { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    /// <summary>True se a assinatura está ativa e dentro do prazo (hoje &lt;= DataFim).</summary>
    public bool AcessoValido() =>
        Status == StatusAssinatura.Ativa && DateTime.Today <= DataFim.Date;
}
