using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

/// <summary>
/// Auditoria de progresso: curso → alunos → aulas concluídas por aluno.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Master")]
public class AuditoriaController : Controller
{
    private readonly ApplicationDbContext _db;

    public AuditoriaController(ApplicationDbContext db) => _db = db;

    // Lista de cursos com nº de alunos e progresso geral.
    public async Task<IActionResult> Index()
    {
        var cursos = await _db.CursosOnline
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .Select(c => new { c.Id, c.Nome })
            .ToListAsync();

        var aulasPorCurso = await _db.Licoes
            .GroupBy(l => l.Modulo!.CursoOnlineId)
            .Select(g => new { CursoId = g.Key, Qtd = g.Count() })
            .ToListAsync();

        var matriculasPorCurso = await _db.Matriculas
            .Where(m => m.Ativo)
            .GroupBy(m => m.CursoOnlineId)
            .Select(g => new { CursoId = g.Key, Qtd = g.Count() })
            .ToListAsync();

        var concluidasPorCurso = await _db.ProgressosLicao
            .Where(p => p.Concluida)
            .GroupBy(p => p.Licao!.Modulo!.CursoOnlineId)
            .Select(g => new { CursoId = g.Key, Qtd = g.Count() })
            .ToListAsync();

        var aulasMap = aulasPorCurso.ToDictionary(x => x.CursoId, x => x.Qtd);
        var matrMap = matriculasPorCurso.ToDictionary(x => x.CursoId, x => x.Qtd);
        var concMap = concluidasPorCurso.ToDictionary(x => x.CursoId, x => x.Qtd);

        var lista = cursos.Select(c =>
        {
            var totalAulas = aulasMap.GetValueOrDefault(c.Id);
            var matriculados = matrMap.GetValueOrDefault(c.Id);
            var concluidas = concMap.GetValueOrDefault(c.Id);
            var denom = matriculados * totalAulas;
            return new AuditoriaCursoItem
            {
                CursoId = c.Id,
                Nome = c.Nome,
                TotalAulas = totalAulas,
                Matriculados = matriculados,
                TotalConcluidas = concluidas,
                PercentualMedio = denom == 0 ? 0 : (int)Math.Round(100.0 * concluidas / denom)
            };
        }).ToList();

        return View(lista);
    }

    // Alunos matriculados em um curso e o progresso de cada um.
    public async Task<IActionResult> Curso(int id)
    {
        var curso = await _db.CursosOnline.FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();

        var totalAulas = await _db.Licoes.CountAsync(l => l.Modulo!.CursoOnlineId == id);

        var matriculas = await _db.Matriculas
            .Where(m => m.CursoOnlineId == id && m.Ativo)
            .Select(m => new { m.AlunoId, Nome = m.Aluno!.NomeCompleto, m.Aluno.Email })
            .ToListAsync();

        var progresso = await _db.ProgressosLicao
            .Where(p => p.Licao!.Modulo!.CursoOnlineId == id)
            .GroupBy(p => p.AlunoId)
            .Select(g => new
            {
                AlunoId = g.Key,
                Iniciadas = g.Count(),
                Concluidas = g.Count(x => x.Concluida),
                UltimoAcesso = g.Max(x => x.UltimoAcesso)
            })
            .ToListAsync();
        var progMap = progresso.ToDictionary(x => x.AlunoId);

        var alunos = matriculas.Select(m =>
        {
            progMap.TryGetValue(m.AlunoId, out var p);
            return new AuditoriaAlunoLinha
            {
                AlunoId = m.AlunoId,
                Nome = m.Nome,
                Email = m.Email,
                TotalAulas = totalAulas,
                Concluidas = p?.Concluidas ?? 0,
                Iniciadas = p?.Iniciadas ?? 0,
                UltimoAcesso = p?.UltimoAcesso
            };
        })
        .OrderByDescending(a => a.Percentual)
        .ThenBy(a => a.Nome)
        .ToList();

        var vm = new AuditoriaCursoViewModel
        {
            CursoId = id,
            CursoNome = curso.Nome,
            TotalAulas = totalAulas,
            Alunos = alunos
        };
        return View(vm);
    }

    // Aula por aula de um aluno em um curso.
    public async Task<IActionResult> Aluno(int cursoId, int alunoId)
    {
        var curso = await _db.CursosOnline
            .Include(c => c.Modulos.OrderBy(m => m.Ordem))
                .ThenInclude(m => m.Licoes.OrderBy(l => l.Ordem))
            .FirstOrDefaultAsync(c => c.Id == cursoId);
        if (curso == null) return NotFound();

        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId);
        if (aluno == null) return NotFound();

        var progressos = await _db.ProgressosLicao
            .Where(p => p.AlunoId == alunoId && p.Licao!.Modulo!.CursoOnlineId == cursoId)
            .ToListAsync();
        var progMap = progressos.ToDictionary(p => p.LicaoId);

        var vm = new AuditoriaAlunoViewModel
        {
            CursoId = cursoId,
            CursoNome = curso.Nome,
            AlunoId = alunoId,
            AlunoNome = aluno.NomeCompleto,
            AlunoEmail = aluno.Email
        };

        foreach (var m in curso.Modulos.OrderBy(x => x.Ordem))
        {
            var grupo = new AuditoriaModuloGrupo { Titulo = m.Titulo };
            foreach (var l in m.Licoes.OrderBy(x => x.Ordem))
            {
                progMap.TryGetValue(l.Id, out var p);
                grupo.Aulas.Add(new AuditoriaAulaLinha
                {
                    Titulo = l.Titulo,
                    Concluida = p?.Concluida ?? false,
                    Iniciada = p != null && !(p.Concluida),
                    UltimoAcesso = p?.UltimoAcesso,
                    TempoAssistidoSegundos = p?.TempoAssistidoSegundos ?? 0
                });
            }
            vm.Modulos.Add(grupo);
        }

        vm.TotalAulas = vm.Modulos.Sum(g => g.Aulas.Count);
        vm.Concluidas = vm.Modulos.Sum(g => g.Aulas.Count(a => a.Concluida));
        vm.UltimoAcesso = progressos.Count == 0 ? null : progressos.Max(p => p.UltimoAcesso);

        return View(vm);
    }
}
