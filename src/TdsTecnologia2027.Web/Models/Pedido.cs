using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Situação de um pedido da loja.</summary>
public enum StatusPedido
{
    Pendente = 0,
    Pago = 1,
    Cancelado = 2
}

/// <summary>
/// Pedido de compra de cursos online. Gerado no checkout (Pendente) e confirmado
/// pelo webhook/retorno do InfinitePay (Pago), quando o acesso é liberado.
/// </summary>
public class Pedido
{
    public int Id { get; set; }

    /// <summary>Identificador enviado ao InfinitePay (order_nsu). Único.</summary>
    [Required]
    [StringLength(60)]
    public string OrderNsu { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [StringLength(150)]
    [Display(Name = "Nome")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string EmailCliente { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Telefone")]
    public string? TelefoneCliente { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    public StatusPedido Status { get; set; } = StatusPedido.Pendente;

    /// <summary>Código da fatura no InfinitePay (invoice_slug).</summary>
    [StringLength(80)]
    public string? InvoiceSlug { get; set; }

    [StringLength(80)]
    public string? TransactionNsu { get; set; }

    [StringLength(30)]
    public string? MetodoPagamento { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? PagoEm { get; set; }

    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();

    public long TotalCentavos => (long)decimal.Round(Total * 100m, 0);
}

/// <summary>Item (curso) dentro de um pedido. Guarda nome e preço no momento da compra.</summary>
public class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    [Required]
    [StringLength(160)]
    public string Descricao { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; }
}
