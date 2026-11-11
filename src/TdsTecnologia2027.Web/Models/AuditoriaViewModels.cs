namespace TdsTecnologia2027.Web.Models;

// ===== Auditoria de progresso (Admin) =====

/// <summary>Linha da lista de cursos na auditoria.</summary>
public class AuditoriaCursoItem
{
    public int CursoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int TotalAulas { get; set; }
    public int Matriculados { get; set; }
    /// <summary>Soma de aulas concluídas por todos os alunos do curso.</summary>
    public int TotalConcluidas { get; set; }
    /// <summary>Progresso médio dos matriculados (0–100).</summary>
    public int PercentualMedio { get; set; }
}

/// <summary>Linha de aluno dentro de um curso (progresso agregado).</summary>
public class AuditoriaAlunoLinha
{
    public int AlunoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Concluidas { get; set; }
    /// <summary>Aulas com algum progresso registrado (acessadas).</summary>
    public int Iniciadas { get; set; }
    public int TotalAulas { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public int Percentual => TotalAulas == 0 ? 0 : (int)Math.Round(100.0 * Concluidas / TotalAulas);
}

/// <summary>Tela: alunos de um curso.</summary>
public class AuditoriaCursoViewModel
{
    public int CursoId { get; set; }
    public string CursoNome { get; set; } = string.Empty;
    public int TotalAulas { get; set; }
    public List<AuditoriaAlunoLinha> Alunos { get; set; } = new();
    public int ComProgresso => Alunos.Count(a => a.Iniciadas > 0);
    public int Concluintes => Alunos.Count(a => a.TotalAulas > 0 && a.Concluidas >= a.TotalAulas);
}

/// <summary>Linha de aula no detalhe do aluno.</summary>
public class AuditoriaAulaLinha
{
    public string Titulo { get; set; } = string.Empty;
    public bool Concluida { get; set; }
    /// <summary>Tem progresso registrado (acessou), mas não concluiu.</summary>
    public bool Iniciada { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public int TempoAssistidoSegundos { get; set; }
}

/// <summary>Grupo de aulas por módulo no detalhe do aluno.</summary>
public class AuditoriaModuloGrupo
{
    public string Titulo { get; set; } = string.Empty;
    public List<AuditoriaAulaLinha> Aulas { get; set; } = new();
}

/// <summary>Tela: aulas de um aluno em um curso.</summary>
public class AuditoriaAlunoViewModel
{
    public int CursoId { get; set; }
    public string CursoNome { get; set; } = string.Empty;
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string? AlunoEmail { get; set; }
    public int TotalAulas { get; set; }
    public int Concluidas { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public int Percentual => TotalAulas == 0 ? 0 : (int)Math.Round(100.0 * Concluidas / TotalAulas);
    public List<AuditoriaModuloGrupo> Modulos { get; set; } = new();
}
