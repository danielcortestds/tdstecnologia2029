using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Voucher de acesso: código que o aluno resgata para liberar um curso online
/// sem comprar pelo site (cortesia, venda externa, parceria, etc.).
/// </summary>
public class Voucher
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    [Display(Name = "Validade (opcional)")]
    [DataType(DataType.Date)]
    public DateTime? Validade { get; set; }

    [Display(Name = "Usos máximos")]
    [Range(1, 100000)]
    public int UsosMaximos { get; set; } = 1;

    [Display(Name = "Usos feitos")]
    public int UsosFeitos { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    [StringLength(200)]
    [Display(Name = "Observação")]
    public string? Observacao { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<VoucherResgate> Resgates { get; set; } = new List<VoucherResgate>();

    /// <summary>True se o voucher ainda pode ser resgatado hoje.</summary>
    public bool Disponivel()
    {
        if (!Ativo) return false;
        if (UsosFeitos >= UsosMaximos) return false;
        if (Validade.HasValue && DateTime.Today > Validade.Value.Date) return false;
        return true;
    }
}

/// <summary>Registro de cada resgate de um voucher (auditoria).</summary>
public class VoucherResgate
{
    public int Id { get; set; }

    public int VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    [Required]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Nome { get; set; }

    public int? AlunoId { get; set; }

    public DateTime ResgatadoEm { get; set; } = DateTime.Now;
}
