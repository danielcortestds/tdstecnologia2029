using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Lição (aula em vídeo) dentro de um módulo.</summary>
public class Licao
{
    public int Id { get; set; }

    public int ModuloId { get; set; }
    public ModuloCurso? Modulo { get; set; }

    [Required(ErrorMessage = "Informe o título da aula.")]
    [StringLength(150)]
    [Display(Name = "Título da aula")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Display(Name = "Ordem")]
    public int Ordem { get; set; }

    [Display(Name = "Duração (minutos)")]
    public int Duracao { get; set; }

    [Required(ErrorMessage = "Informe a URL do vídeo no Vimeo.")]
    [StringLength(300)]
    [Display(Name = "URL do vídeo (Vimeo)")]
    public string UrlVideoVimeo { get; set; } = string.Empty;
}
