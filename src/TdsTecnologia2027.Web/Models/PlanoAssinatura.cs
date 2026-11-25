using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Produto de assinatura (ex.: "TDS Pass"): acesso a todos os cursos online por
/// um período. Normalmente existe um único registro ativo. A compra gera uma
/// <see cref="Assinatura"/> para o aluno.
/// </summary>
public class PlanoAssinatura
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nome do plano")]
    public string Nome { get; set; } = "TDS Pass";

    [Required]
    [StringLength(80)]
    [Display(Name = "Slug (URL)")]
    public string Slug { get; set; } = "tds-pass";

    [StringLength(600)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Range(0, 999999)]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Preço (R$)")]
    public decimal Preco { get; set; }

    /// <summary>Preço "de" (riscado), âncora opcional.</summary>
    [Range(0, 999999)]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Preço original (de)")]
    public decimal? PrecoDe { get; set; }

    [Range(1, 120)]
    [Display(Name = "Duração base (meses)")]
    public int DuracaoMeses { get; set; } = 12;

    [Range(0, 120)]
    [Display(Name = "Bônus (meses)")]
    public int BonusMeses { get; set; } = 6;

    /// <summary>Liga o bônus de lançamento: quando true, novas assinaturas ganham os meses de bônus.</summary>
    [Display(Name = "Bônus ativo (lançamento)")]
    public bool BonusAtivo { get; set; } = true;

    /// <summary>Plano disponível para compra no site.</summary>
    [Display(Name = "À venda")]
    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }

    /// <summary>Meses efetivos de acesso na compra (base + bônus, se ativo).</summary>
    public int MesesTotais() => DuracaoMeses + (BonusAtivo ? BonusMeses : 0);

    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
