namespace TdsTecnologia2027.Web.Models;

/// <summary>Progresso de um aluno em uma lição (aula).</summary>
public class ProgressoLicao
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int LicaoId { get; set; }
    public Licao? Licao { get; set; }

    /// <summary>Aula marcada como concluída/assistida.</summary>
    public bool Concluida { get; set; }

    /// <summary>Tempo assistido acumulado, em segundos.</summary>
    public int TempoAssistidoSegundos { get; set; }

    public DateTime UltimoAcesso { get; set; } = DateTime.Now;
}
