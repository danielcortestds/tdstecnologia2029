namespace TdsTecnologia2027.Web.Models;

/// <summary>Card de curso no dashboard "Meus Cursos".</summary>
public class MeuCursoViewModel
{
    public CursoOnline Curso { get; set; } = null!;
    public int PercentualConcluido { get; set; }
    public int? ContinuarLicaoId { get; set; }
    public DateTime? UltimoAcesso { get; set; }
}

public enum StatusLicaoAluno { NaoIniciada, EmAndamento, Assistida }

public class LicaoSidebarViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int Duracao { get; set; }
    public StatusLicaoAluno Status { get; set; }
    public bool Atual { get; set; }
}

public class ModuloSidebarViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public List<LicaoSidebarViewModel> Licoes { get; set; } = new();
}

/// <summary>Tela de assistir aula (player + barra lateral + navegação).</summary>
public class AssistirViewModel
{
    public CursoOnline Curso { get; set; } = null!;
    public Licao LicaoAtual { get; set; } = null!;
    public string? EmbedUrl { get; set; }
    public List<ModuloSidebarViewModel> Modulos { get; set; } = new();
    public int? AnteriorLicaoId { get; set; }
    public int? ProximaLicaoId { get; set; }
    public int PercentualConcluido { get; set; }
    public bool LicaoConcluida { get; set; }
}
