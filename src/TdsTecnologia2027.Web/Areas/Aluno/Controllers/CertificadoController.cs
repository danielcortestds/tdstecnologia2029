using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

public class CertificadoController : AlunoBaseController
{
    private readonly CertificadoService _cert;

    public CertificadoController(ApplicationDbContext db, CertificadoService cert) : base(db)
    {
        _cert = cert;
    }

    // POST: /Aluno/Certificado/Emitir/{cursoId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(int id)
    {
        var (aluno, curso, erro) = await CarregarAsync(id);
        if (erro != null) return View("Aviso", erro);

        var cert = await _cert.ObterOuEmitirAsync(aluno!, curso!);
        if (cert == null)
            return View("Aviso", $"Você precisa concluir pelo menos {Certificado.PercentualMinimo}% do curso para emitir o certificado.");

        return RedirectToAction(nameof(Ver), new { id });
    }

    // GET: /Aluno/Certificado/Ver/{id}  (id = curso)
    [HttpGet]
    public async Task<IActionResult> Ver(int id)
    {
        var (aluno, curso, erro) = await CarregarAsync(id);
        if (erro != null) return View("Aviso", erro);

        var cert = await _cert.ObterDoAlunoAsync(aluno!.Id, id);
        if (cert == null)
            return View("Aviso", $"Certificado ainda não emitido. Conclua pelo menos {Certificado.PercentualMinimo}% do curso.");

        var urlValidacao = _cert.UrlValidacao(cert.CodigoValidacao);
        ViewBag.UrlValidacao = urlValidacao;
        ViewBag.UrlLinkedIn = MontarLinkedIn(cert, urlValidacao);
        return View(cert);
    }

    // GET: /Aluno/Certificado/Baixar/{id}  (id = curso)
    [HttpGet]
    public async Task<IActionResult> Baixar(int id)
    {
        var (aluno, curso, erro) = await CarregarAsync(id);
        if (erro != null) return View("Aviso", erro);

        var cert = await _cert.ObterDoAlunoAsync(aluno!.Id, id);
        if (cert == null)
            return View("Aviso", "Certificado não encontrado. Emita o certificado primeiro.");

        var pdf = _cert.GerarPdf(cert, curso!);
        var nome = $"Certificado-{Slug(cert.CursoNome)}-{cert.CodigoValidacao}.pdf";
        return File(pdf, "application/pdf", nome);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private async Task<(Models.Aluno? aluno, CursoOnline? curso, string? erro)> CarregarAsync(int id)
    {
        var aluno = await ObterAlunoAsync();
        if (aluno == null || aluno.Status != StatusAluno.Ativo)
            return (null, null, "Seu acesso está indisponível.");

        var matricula = await Db.Matriculas.FirstOrDefaultAsync(m => m.AlunoId == aluno.Id && m.CursoOnlineId == id);
        if (matricula == null || !matricula.AcessoValido())
            return (null, null, "Você não tem acesso a este curso.");

        var curso = await Db.CursosOnline.FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return (null, null, "Curso não encontrado.");

        return (aluno, curso, null);
    }

    private static string MontarLinkedIn(Certificado cert, string urlValidacao)
    {
        string E(string s) => Uri.EscapeDataString(s);
        return "https://www.linkedin.com/profile/add?startTask=CERTIFICATION_NAME"
            + $"&name={E(cert.CursoNome)}"
            + "&organizationName=" + E("TDS Tecnologia")
            + $"&issueYear={cert.EmitidoEm.Year}&issueMonth={cert.EmitidoEm.Month}"
            + $"&certId={E(cert.CodigoValidacao)}"
            + $"&certUrl={E(urlValidacao)}";
    }

    private static string Slug(string s)
    {
        var limpo = new string(s.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
        return limpo.Trim().Replace(' ', '-');
    }
}
