using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

public class HomeController : AlunoBaseController
{
    public HomeController(ApplicationDbContext db) : base(db) { }

    // GET: /Aluno  → dashboard "Meus Cursos"
    public async Task<IActionResult> Index()
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null) return View("Aviso", "Não encontramos o seu perfil de aluno. Fale com o suporte.");
        if (aluno.Status != StatusAluno.Ativo)
            return View("Aviso", "Seu acesso está inativo ou bloqueado. Entre em contato com a TDS Tecnologia.");

        var matriculas = await Db.Matriculas
            .Include(m => m.CursoOnline!).ThenInclude(c => c.Modulos).ThenInclude(mo => mo.Licoes)
            .Where(m => m.AlunoId == aluno.Id)
            .ToListAsync();

        var progressos = await Db.ProgressosLicao.Where(p => p.AlunoId == aluno.Id).ToListAsync();

        var cursos = new List<MeuCursoViewModel>();
        foreach (var m in matriculas)
        {
            var curso = m.CursoOnline!;
            if (!m.AcessoValido() || curso.Status != StatusCursoOnline.Publicado) continue;

            var licoesOrdenadas = curso.Modulos.OrderBy(mo => mo.Ordem)
                .SelectMany(mo => mo.Licoes.OrderBy(l => l.Ordem)).ToList();
            var total = licoesOrdenadas.Count;
            var ids = licoesOrdenadas.Select(l => l.Id).ToHashSet();
            var concluidas = progressos.Count(p => p.Concluida && ids.Contains(p.LicaoId));
            var pct = total > 0 ? (int)Math.Round(concluidas * 100.0 / total) : 0;

            var ultimo = progressos.Where(p => ids.Contains(p.LicaoId))
                .OrderByDescending(p => p.UltimoAcesso).FirstOrDefault();
            var continuar = ultimo?.LicaoId ?? licoesOrdenadas.FirstOrDefault()?.Id;

            cursos.Add(new MeuCursoViewModel
            {
                Curso = curso,
                PercentualConcluido = pct,
                ContinuarLicaoId = continuar,
                UltimoAcesso = ultimo?.UltimoAcesso
            });
        }

        return View(cursos.OrderByDescending(c => c.UltimoAcesso ?? DateTime.MinValue).ToList());
    }
}
