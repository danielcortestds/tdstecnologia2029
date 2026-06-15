namespace TdsTecnologia2027.Web.Models;

public class DashboardLmsViewModel
{
    public int TotalCursos { get; set; }
    public int CursosPublicados { get; set; }
    public int TotalAlunos { get; set; }
    public int AlunosAtivos { get; set; }
    public int TotalAulas { get; set; }
    public int AulasAssistidas { get; set; }

    public List<ContagemItem> AlunosPorCurso { get; set; } = new();
    public List<ContagemItem> CursosMaisAcessados { get; set; } = new();
    public List<AcessoRecente> UltimosAcessos { get; set; } = new();
}

public class ContagemItem
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class AcessoRecente
{
    public string Aluno { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public string Licao { get; set; } = string.Empty;
    public DateTime Quando { get; set; }
}
