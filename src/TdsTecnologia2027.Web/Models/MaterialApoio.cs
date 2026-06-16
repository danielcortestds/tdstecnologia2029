using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Arquivo de material de apoio (PDF/ZIP) de um curso online, para download.</summary>
public class MaterialApoio
{
    public int Id { get; set; }

    public int CursoOnlineId { get; set; }
    public CursoOnline? CursoOnline { get; set; }

    [StringLength(150)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Nome original do arquivo (usado no download).</summary>
    [StringLength(260)]
    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>Nome físico no disco (GUID + extensão).</summary>
    [StringLength(100)]
    public string ArquivoArmazenado { get; set; } = string.Empty;

    /// <summary>"pdf" ou "zip".</summary>
    [StringLength(10)]
    public string Tipo { get; set; } = string.Empty;

    public long TamanhoBytes { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
