using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

public class CursoController : AlunoBaseController
{
    public CursoController(ApplicationDbContext db) : base(db) { }

    // GET: /Aluno/Curso/Assistir/{id}?licao={licaoId}
    public async Task<IActionResult> Assistir(int id, int? licao)
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null || aluno.Status != StatusAluno.Ativo)
            return View("Aviso", "Seu acesso está indisponível. Entre em contato com a TDS Tecnologia.");

        var matricula = await Db.Matriculas.FirstOrDefaultAsync(m => m.AlunoId == aluno.Id && m.CursoOnlineId == id);
        if (matricula == null || !matricula.AcessoValido())
            return View("Aviso", "Você não tem acesso a este curso (ou o prazo expirou).");

        var curso = await Db.CursosOnline
            .Include(c => c.Modulos.OrderBy(m => m.Ordem)).ThenInclude(m => m.Licoes.OrderBy(l => l.Ordem))
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null || curso.Status != StatusCursoOnline.Publicado) return NotFound();

        var ordenadas = curso.Modulos.SelectMany(m => m.Licoes).ToList();
        if (ordenadas.Count == 0) return View("Aviso", "Este curso ainda não tem aulas disponíveis.");

        var progressos = await Db.ProgressosLicao.Where(p => p.AlunoId == aluno.Id).ToListAsync();

        // Lição atual: a informada (se válida), senão a última acessada, senão a primeira.
        var atual = licao.HasValue ? ordenadas.FirstOrDefault(l => l.Id == licao.Value) : null;
        if (atual == null)
        {
            var ultimoProg = progressos.Where(p => ordenadas.Any(l => l.Id == p.LicaoId))
                .OrderByDescending(p => p.UltimoAcesso).FirstOrDefault();
            atual = ordenadas.FirstOrDefault(l => l.Id == ultimoProg?.LicaoId) ?? ordenadas.First();
        }

        // Marca a aula atual como "em andamento" (registra acesso).
        var prog = progressos.FirstOrDefault(p => p.LicaoId == atual.Id);
        if (prog == null)
        {
            prog = new ProgressoLicao { AlunoId = aluno.Id, LicaoId = atual.Id, UltimoAcesso = DateTime.Now };
            Db.ProgressosLicao.Add(prog);
            progressos.Add(prog);
        }
        else
        {
            prog.UltimoAcesso = DateTime.Now;
        }
        await Db.SaveChangesAsync();

        // Barra lateral com status por aula
        var modulosVm = curso.Modulos.OrderBy(m => m.Ordem).Select(m => new ModuloSidebarViewModel
        {
            Titulo = m.Titulo,
            Licoes = m.Licoes.OrderBy(l => l.Ordem).Select(l => new LicaoSidebarViewModel
            {
                Id = l.Id,
                Titulo = l.Titulo,
                Duracao = l.Duracao,
                Atual = l.Id == atual.Id,
                Status = StatusDaLicao(progressos, l.Id)
            }).ToList()
        }).ToList();

        var idx = ordenadas.FindIndex(l => l.Id == atual.Id);
        var total = ordenadas.Count;
        var concluidas = progressos.Count(p => p.Concluida && ordenadas.Any(l => l.Id == p.LicaoId));

        var vm = new AssistirViewModel
        {
            Curso = curso,
            LicaoAtual = atual,
            EmbedUrl = VimeoHelper.ObterUrlEmbed(atual.UrlVideoVimeo),
            Modulos = modulosVm,
            AnteriorLicaoId = idx > 0 ? ordenadas[idx - 1].Id : null,
            ProximaLicaoId = idx < total - 1 ? ordenadas[idx + 1].Id : null,
            PercentualConcluido = total > 0 ? (int)Math.Round(concluidas * 100.0 / total) : 0,
            LicaoConcluida = prog.Concluida
        };
        return View(vm);
    }

    // POST: marca aula como concluída (botão ou evento "ended" do player)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int licaoId)
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null) return Unauthorized();

        var licao = await Db.Licoes.Include(l => l.Modulo).FirstOrDefaultAsync(l => l.Id == licaoId);
        if (licao == null) return NotFound();

        // valida acesso ao curso
        var cursoId = licao.Modulo!.CursoOnlineId;
        var matricula = await Db.Matriculas.FirstOrDefaultAsync(m => m.AlunoId == aluno.Id && m.CursoOnlineId == cursoId);
        if (matricula == null || !matricula.AcessoValido()) return Forbid();

        var prog = await Db.ProgressosLicao.FirstOrDefaultAsync(p => p.AlunoId == aluno.Id && p.LicaoId == licaoId);
        if (prog == null)
        {
            prog = new ProgressoLicao { AlunoId = aluno.Id, LicaoId = licaoId };
            Db.ProgressosLicao.Add(prog);
        }
        prog.Concluida = true;
        prog.UltimoAcesso = DateTime.Now;
        await Db.SaveChangesAsync();

        // Avança para a próxima aula, se houver
        var proxima = await ProximaLicaoIdAsync(cursoId, licaoId);
        return RedirectToAction(nameof(Assistir), new { id = cursoId, licao = proxima ?? licaoId });
    }

    // POST AJAX: acumula tempo assistido (chamado periodicamente pelo player)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarTempo(int licaoId, int segundos)
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null) return Unauthorized();

        var prog = await Db.ProgressosLicao.FirstOrDefaultAsync(p => p.AlunoId == aluno.Id && p.LicaoId == licaoId);
        if (prog == null)
        {
            prog = new ProgressoLicao { AlunoId = aluno.Id, LicaoId = licaoId };
            Db.ProgressosLicao.Add(prog);
        }
        if (segundos > prog.TempoAssistidoSegundos) prog.TempoAssistidoSegundos = segundos;
        prog.UltimoAcesso = DateTime.Now;
        await Db.SaveChangesAsync();
        return Ok();
    }

    private static StatusLicaoAluno StatusDaLicao(List<ProgressoLicao> progressos, int licaoId)
    {
        var p = progressos.FirstOrDefault(x => x.LicaoId == licaoId);
        if (p == null) return StatusLicaoAluno.NaoIniciada;
        return p.Concluida ? StatusLicaoAluno.Assistida : StatusLicaoAluno.EmAndamento;
    }

    private async Task<int?> ProximaLicaoIdAsync(int cursoId, int licaoIdAtual)
    {
        var ordenadas = await Db.Licoes
            .Where(l => l.Modulo!.CursoOnlineId == cursoId)
            .OrderBy(l => l.Modulo!.Ordem).ThenBy(l => l.Ordem)
            .Select(l => l.Id)
            .ToListAsync();
        var idx = ordenadas.IndexOf(licaoIdAtual);
        return (idx >= 0 && idx < ordenadas.Count - 1) ? ordenadas[idx + 1] : null;
    }
}
