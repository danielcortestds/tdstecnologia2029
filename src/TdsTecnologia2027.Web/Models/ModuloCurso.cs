using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Módulo de um curso online — agrupa lições.</summary>
public class ModuloCurso
{
    public int Id { get; set; }

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    [Required(ErrorMessage = "Informe o título do módulo.")]
    [StringLength(150)]
    [Display(Name = "Título do módulo")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Ordem")]
    public int Ordem { get; set; }

    public ICollection<Licao> Licoes { get; set; } = new List<Licao>();
}
