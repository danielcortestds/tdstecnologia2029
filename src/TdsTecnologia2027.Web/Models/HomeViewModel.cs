namespace TdsTecnologia2027.Web.Models;

/// <summary>
/// Dados da página principal: cursos online à venda (destaque) e cursos
/// presenciais (bloco secundário / turmas fechadas).
/// </summary>
public class HomeViewModel
{
    public IReadOnlyList<CursoOnline> CursosOnline { get; set; } = new List<CursoOnline>();
    public IReadOnlyList<Curso> CursosPresenciais { get; set; } = new List<Curso>();
}
