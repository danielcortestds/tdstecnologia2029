using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardLmsViewModel
        {
            TotalCursos = await _db.CursosOnline.CountAsync(),
            CursosPublicados = await _db.CursosOnline.CountAsync(c => c.Status == StatusCursoOnline.Publicado),
            TotalAlunos = await _db.Alunos.CountAsync(),
            AlunosAtivos = await _db.Alunos.CountAsync(a => a.Status == StatusAluno.Ativo),
            TotalAulas = await _db.Licoes.CountAsync(),
            AulasAssistidas = await _db.ProgressosLicao.CountAsync(p => p.Concluida)
        };

        // Alunos por curso (matrículas ativas)
        vm.AlunosPorCurso = await _db.Matriculas
            .Where(m => m.Ativo)
            .GroupBy(m => m.CursoOnline!.Nome)
            .Select(g => new ContagemItem { Nome = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(10)
            .ToListAsync();

        // Cursos mais acessados (por nº de progressos registrados)
        vm.CursosMaisAcessados = await _db.ProgressosLicao
            .GroupBy(p => p.Licao!.Modulo!.CursoOnline!.Nome)
            .Select(g => new ContagemItem { Nome = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(10)
            .ToListAsync();

        // Últimos acessos
        vm.UltimosAcessos = await _db.ProgressosLicao
            .OrderByDescending(p => p.UltimoAcesso)
            .Take(10)
            .Select(p => new AcessoRecente
            {
                Aluno = p.Aluno!.NomeCompleto,
                Curso = p.Licao!.Modulo!.CursoOnline!.Nome,
                Licao = p.Licao!.Titulo,
                Quando = p.UltimoAcesso
            })
            .ToListAsync();

        return View(vm);
    }
}
