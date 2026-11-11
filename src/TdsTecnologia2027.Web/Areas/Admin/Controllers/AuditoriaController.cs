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

    // Lista de alunos (visão por aluno) com nº de cursos e progresso geral.
    public async Task<IActionResult> Alunos()
    {
        var matriculas = await _db.Matriculas
            .Where(m => m.Ativo)
            .Select(m => new { m.AlunoId, Nome = m.Aluno!.NomeCompleto, m.Aluno.Email, m.CursoOnlineId })
            .ToListAsync();

        var aulasMap = (await _db.Licoes
                .GroupBy(l => l.Modulo!.CursoOnlineId)
                .Select(g => new { CursoId = g.Key, Qtd = g.Count() })
                .ToListAsync())
            .ToDictionary(x => x.CursoId, x => x.Qtd);

        // Progresso achatado (sem GroupBy no SQL) — agregado em memória.
        var prog = await _db.ProgressosLicao
            .Select(p => new { p.AlunoId, CursoId = p.Licao!.Modulo!.CursoOnlineId, p.Concluida, p.UltimoAcesso })
            .ToListAsync();

        var lista = matriculas
            .GroupBy(m => new { m.AlunoId, m.Nome, m.Email })
            .Select(g =>
            {
                var cursoIds = g.Select(x => x.CursoOnlineId).Distinct().ToList();
                var totalAulas = cursoIds.Sum(cid => aulasMap.GetValueOrDefault(cid));
                var meus = prog.Where(p => p.AlunoId == g.Key.AlunoId && cursoIds.Contains(p.CursoId)).ToList();
                return new AuditoriaAlunoGeralItem
                {
                    AlunoId = g.Key.AlunoId,
                    Nome = g.Key.Nome,
                    Email = g.Key.Email,
                    Cursos = cursoIds.Count,
                    TotalAulas = totalAulas,
                    Concluidas = meus.Count(p => p.Concluida),
                    UltimoAcesso = meus.Count > 0 ? meus.Max(p => p.UltimoAcesso) : null
                };
            })
            .OrderBy(a => a.Nome)
            .ToList();

        return View(lista);
    }

    // Perfil do aluno: cursos que faz (com %) + histórico recente de aulas.
    public async Task<IActionResult> Perfil(int alunoId)
    {
        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId);
        if (aluno == null) return NotFound();

        var matriculas = await _db.Matriculas
            .Where(m => m.AlunoId == alunoId && m.Ativo)
            .Select(m => new { m.CursoOnlineId, Nome = m.CursoOnline!.Nome, m.CursoOnline.Ordem })
            .ToListAsync();

        var aulasMap = (await _db.Licoes
                .GroupBy(l => l.Modulo!.CursoOnlineId)
                .Select(g => new { CursoId = g.Key, Qtd = g.Count() })
                .ToListAsync())
            .ToDictionary(x => x.CursoId, x => x.Qtd);

        var progressos = await _db.ProgressosLicao
            .Where(p => p.AlunoId == alunoId)
            .OrderByDescending(p => p.UltimoAcesso)
            .Select(p => new
            {
                CursoId = p.Licao!.Modulo!.CursoOnlineId,
                Curso = p.Licao.Modulo.CursoOnline!.Nome,
                Aula = p.Licao.Titulo,
                p.UltimoAcesso,
                p.Concluida
            })
            .ToListAsync();

        var vm = new AuditoriaPerfilViewModel
        {
            AlunoId = alunoId,
            Nome = aluno.NomeCompleto,
            Email = aluno.Email
        };

        vm.Cursos = matriculas
            .OrderBy(m => m.Ordem).ThenBy(m => m.Nome)
            .Select(m =>
            {
                var meus = progressos.Where(p => p.CursoId == m.CursoOnlineId).ToList();
                return new AuditoriaPerfilCurso
                {
                    CursoId = m.CursoOnlineId,
                    Nome = m.Nome,
                    TotalAulas = aulasMap.GetValueOrDefault(m.CursoOnlineId),
                    Concluidas = meus.Count(p => p.Concluida),
                    UltimoAcesso = meus.Count > 0 ? meus.Max(p => p.UltimoAcesso) : null,
                    UltimaAula = meus.Count > 0 ? meus.First().Aula : null
                };
            })
            .ToList();

        vm.Historico = progressos
            .Take(12)
            .Select(p => new AuditoriaHistoricoItem
            {
                Curso = p.Curso,
                Aula = p.Aula,
                Quando = p.UltimoAcesso,
                Concluida = p.Concluida
            })
            .ToList();

        vm.TotalCursos = vm.Cursos.Count;
        vm.TotalAulas = vm.Cursos.Sum(c => c.TotalAulas);
        vm.Concluidas = vm.Cursos.Sum(c => c.Concluidas);
        vm.UltimoAcesso = progressos.Count > 0 ? progressos.Max(p => p.UltimoAcesso) : null;

        return View(vm);
    }
}
