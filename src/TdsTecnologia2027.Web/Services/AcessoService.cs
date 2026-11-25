using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Decide se um aluno tem acesso a um curso online. Acesso =
/// <b>matrícula válida</b> (compra avulsa) <b>OU assinatura ativa</b> (TDS Pass).
/// Centraliza a regra usada pelo player, certificado e dashboard do aluno.
/// </summary>
public class AcessoService
{
    private readonly ApplicationDbContext _db;
    public AcessoService(ApplicationDbContext db) => _db = db;

    /// <summary>True se o aluno tem alguma assinatura ativa e dentro do prazo (hoje).</summary>
    public Task<bool> AssinaturaAtivaAsync(int alunoId)
    {
        var hoje = DateTime.Today;
        return _db.Assinaturas.AnyAsync(a =>
            a.AlunoId == alunoId && a.Status == StatusAssinatura.Ativa && a.DataFim >= hoje);
    }

    /// <summary>True se o aluno pode acessar o curso: matrícula válida OU assinatura ativa.</summary>
    public async Task<bool> TemAcessoAsync(int alunoId, int cursoOnlineId)
    {
        var mat = await _db.Matriculas
            .FirstOrDefaultAsync(m => m.AlunoId == alunoId && m.CursoOnlineId == cursoOnlineId);
        if (mat != null && mat.AcessoValido()) return true;
        return await AssinaturaAtivaAsync(alunoId);
    }
}
