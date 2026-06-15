using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Situação de publicação de um curso online.</summary>
public enum StatusCursoOnline
{
    Rascunho = 0,
    Publicado = 1,
    Oculto = 2
}

/// <summary>
/// Curso online (LMS / Área do Aluno) — separado dos cursos presenciais (Curso).
/// </summary>
public class CursoOnline
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do curso.")]
    [StringLength(150)]
    [Display(Name = "Nome do curso")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Slug (URL)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descrição curta")]
    public string? DescricaoCurta { get; set; }

    [StringLength(4000)]
    [Display(Name = "Descrição completa")]
    public string? DescricaoCompleta { get; set; }

    [StringLength(300)]
    [Display(Name = "Imagem de capa (URL)")]
    public string? ImagemCapa { get; set; }

    [StringLength(100)]
    [Display(Name = "Categoria")]
    public string? Categoria { get; set; }

    [StringLength(150)]
    [Display(Name = "Professor/Instrutor")]
    public string? Instrutor { get; set; }

    [Display(Name = "Status")]
    public StatusCursoOnline Status { get; set; } = StatusCursoOnline.Rascunho;

    [Display(Name = "Ordem de exibição")]
    public int Ordem { get; set; }

    [Display(Name = "Carga horária total (horas)")]
    public int CargaHoraria { get; set; }

    [Display(Name = "Emite certificado")]
    public bool Certificado { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<ModuloCurso> Modulos { get; set; } = new List<ModuloCurso>();
}
