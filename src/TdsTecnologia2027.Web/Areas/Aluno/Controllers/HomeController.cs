using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

public class HomeController : AlunoBaseController
{
    private readonly AcessoService _acesso;

    public HomeController(ApplicationDbContext db, AcessoService acesso) : base(db)
    {
        _acesso = acesso;
    }

    // Logout do aluno. Fica neste controller (autenticado no esquema "Aluno") para
    // que o token antiforgery seja validado sob o mesmo usuário que o gerou.
    [HttpPost("/Aluno/Sair")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await AlunoAuth.SairAsync(HttpContext);
        return Redirect("/Aluno/Entrar");
    }

    // GET: /Aluno  → dashboard "Meus Cursos"
    public async Task<IActionResult> Index()
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null) return View("Aviso", "Não encontramos o seu perfil de aluno. Fale com o suporte.");
        if (aluno.Status != StatusAluno.Ativo)
            return View("Aviso", "Seu acesso está inativo ou bloqueado. Entre em contato com a TDS Tecnologia.");

        // Assinante (TDS Pass) enxerga TODOS os cursos publicados; senão, só os matriculados.
        var assinante = await _acesso.AssinaturaAtivaAsync(aluno.Id);

        List<CursoOnline> cursosAcesso;
        if (assinante)
        {
            cursosAcesso = await Db.CursosOnline
                .Include(c => c.Modulos).ThenInclude(mo => mo.Licoes)
                .Where(c => c.Status == StatusCursoOnline.Publicado)
                .OrderBy(c => c.Ordem)
                .ToListAsync();
        }
        else
        {
            var matriculas = await Db.Matriculas
                .Include(m => m.CursoOnline!).ThenInclude(c => c.Modulos).ThenInclude(mo => mo.Licoes)
                .Where(m => m.AlunoId == aluno.Id)
                .ToListAsync();
            cursosAcesso = matriculas
                .Where(m => m.AcessoValido() && m.CursoOnline!.Status == StatusCursoOnline.Publicado)
                .Select(m => m.CursoOnline!)
                .ToList();
        }

        var progressos = await Db.ProgressosLicao.Where(p => p.AlunoId == aluno.Id).ToListAsync();

        var cursos = new List<MeuCursoViewModel>();
        foreach (var curso in cursosAcesso)
        {
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

        // Dados do Pass para o cabeçalho do dashboard (badge "TDS Pass ativo").
        ViewBag.Assinante = assinante;
        if (assinante)
        {
            var ass = await Db.Assinaturas
                .Where(a => a.AlunoId == aluno.Id && a.Status == StatusAssinatura.Ativa)
                .OrderByDescending(a => a.DataFim).FirstOrDefaultAsync();
            ViewBag.PassValidade = ass?.DataFim;
        }

        return View(cursos.OrderByDescending(c => c.UltimoAcesso ?? DateTime.MinValue).ToList());
    }
}
