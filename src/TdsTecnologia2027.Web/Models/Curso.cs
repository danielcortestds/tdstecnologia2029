using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Representa um curso ofertado pela TDS Tecnologia.
/// Os campos abaixo alimentam tanto o card da onepage quanto o modal de detalhes.
/// </summary>
public class Curso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Identificador único usado nas âncoras dos modais (ex: "powerbi").</summary>
    [Required(ErrorMessage = "O slug é obrigatório.")]
    [MaxLength(80, ErrorMessage = "O slug deve ter no máximo 80 caracteres.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Use apenas letras minúsculas, números e hífens.")]
    [Display(Name = "Slug (URL)")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição curta é obrigatória.")]
    [MaxLength(500, ErrorMessage = "A descrição curta deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição curta (card)")]
    public string DescricaoCurta { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    [Display(Name = "Descrição completa (modal)")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Tag exibida acima do título no modal (ex: "Análise de Dados").</summary>
    [MaxLength(100)]
    [Display(Name = "Categoria / Tag do modal")]
    public string? Categoria { get; set; }

    /// <summary>Classe Font Awesome do ícone do card (ex: "fa-solid fa-chart-bar").</summary>
    [MaxLength(80)]
    [Display(Name = "Ícone (classes Font Awesome)")]
    public string? Icone { get; set; }

    [Required(ErrorMessage = "A carga horária é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A carga horária deve ser maior que zero.")]
    [Display(Name = "Carga Horária (horas)")]
    public int CargaHoraria { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser maior ou igual a zero.")]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Preço (R$)")]
    public decimal Preco { get; set; }

    [MaxLength(300)]
    [Display(Name = "URL da Imagem")]
    public string? ImagemUrl { get; set; }

    [Display(Name = "Data de início da turma")]
    [DataType(DataType.Date)]
    public DateTime? DataInicio { get; set; }

    /// <summary>Texto da pill secundária no card (ex: "10x sem juros" ou "In Company").</summary>
    [MaxLength(60)]
    [Display(Name = "Texto da pill secundária")]
    public string? PillSecundaria { get; set; }

    /// <summary>Ícone Font Awesome da pill secundária (ex: "fa-solid fa-credit-card").</summary>
    [MaxLength(80)]
    [Display(Name = "Ícone da pill secundária")]
    public string? IconePillSecundaria { get; set; }

    /// <summary>Tópicos do modal (um item por linha — viram bullets).</summary>
    [MaxLength(2000)]
    [Display(Name = "Tópicos do modal (um por linha)")]
    public string? Topicos { get; set; }

    /// <summary>Texto pré-preenchido no link wa.me.</summary>
    [MaxLength(300)]
    [Display(Name = "Mensagem do WhatsApp")]
    public string? WhatsAppMensagem { get; set; }

    [Display(Name = "Ordem de exibição")]
    public int Ordem { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    [Display(Name = "Criado em")]
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    [Display(Name = "Atualizado em")]
    public DateTime? AtualizadoEm { get; set; }
}
