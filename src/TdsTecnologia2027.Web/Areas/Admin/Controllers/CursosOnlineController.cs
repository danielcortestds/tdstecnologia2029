using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master,Editor")]
public class CursosOnlineController : Controller
{
    private readonly ApplicationDbContext _db;

    public CursosOnlineController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── Cursos ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var cursos = await _db.CursosOnline
            .Include(c => c.Modulos)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Id)
            .ToListAsync();
        return View(cursos);
    }

    public IActionResult Create() => View(new CursoOnline());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CursoOnline model)
    {
        model.Slug = await GerarSlugUnicoAsync(model.Slug, model.Nome, null);
        ModelState.Remove(nameof(CursoOnline.Slug)); // gerado automaticamente
        if (!ModelState.IsValid) return View(model);

        model.CriadoEm = DateTime.Now;
        _db.CursosOnline.Add(model);
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Curso online criado! Agora adicione os módulos e aulas.";
        return RedirectToAction(nameof(Conteudo), new { id = model.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var curso = await _db.CursosOnline.FindAsync(id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CursoOnline model)
    {
        if (id != model.Id) return NotFound();
        model.Slug = await GerarSlugUnicoAsync(model.Slug, model.Nome, model.Id);
        ModelState.Remove(nameof(CursoOnline.Slug)); // gerado automaticamente
        if (!ModelState.IsValid) return View(model);

        var curso = await _db.CursosOnline.FindAsync(id);
        if (curso == null) return NotFound();

        curso.Nome = model.Nome;
        curso.Slug = model.Slug;
        curso.DescricaoCurta = model.DescricaoCurta;
        curso.DescricaoCompleta = model.DescricaoCompleta;
        curso.ImagemCapa = model.ImagemCapa;
        curso.Categoria = model.Categoria;
        curso.Instrutor = model.Instrutor;
        curso.Status = model.Status;
        curso.Ordem = model.Ordem;
        curso.CargaHoraria = model.CargaHoraria;
        curso.Certificado = model.Certificado;
        curso.AtualizadoEm = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Curso online atualizado!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var curso = await _db.CursosOnline.FindAsync(id);
        if (curso != null)
        {
            _db.CursosOnline.Remove(curso); // cascata remove módulos e lições
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Curso online excluído.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Conteúdo: módulos e lições ────────────────────────────────────────
    public async Task<IActionResult> Conteudo(int id)
    {
        var curso = await _db.CursosOnline
            .Include(c => c.Modulos.OrderBy(m => m.Ordem))
                .ThenInclude(m => m.Licoes.OrderBy(l => l.Ordem))
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModulo(int cursoId, string titulo)
    {
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            var proximaOrdem = await _db.Modulos.Where(m => m.CursoOnlineId == cursoId).CountAsync() + 1;
            _db.Modulos.Add(new ModuloCurso { CursoOnlineId = cursoId, Titulo = titulo.Trim(), Ordem = proximaOrdem });
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Módulo adicionado.";
        }
        return RedirectToAction(nameof(Conteudo), new { id = cursoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditModulo(int id, string titulo, int ordem)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        if (modulo != null && !string.IsNullOrWhiteSpace(titulo))
        {
            modulo.Titulo = titulo.Trim();
            modulo.Ordem = ordem;
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Módulo atualizado.";
        }
        return RedirectToAction(nameof(Conteudo), new { id = modulo?.CursoOnlineId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModulo(int id)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        var cursoId = modulo?.CursoOnlineId ?? 0;
        if (modulo != null)
        {
            _db.Modulos.Remove(modulo);
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Módulo excluído.";
        }
        return RedirectToAction(nameof(Conteudo), new { id = cursoId });
    }

    // GET: formulário de aula (criar/editar)
    public async Task<IActionResult> Licao(int? id, int? moduloId)
    {
        if (id is not null)
        {
            var licao = await _db.Licoes.Include(l => l.Modulo).FirstOrDefaultAsync(l => l.Id == id);
            if (licao == null) return NotFound();
            ViewBag.CursoId = licao.Modulo!.CursoOnlineId;
            return View(licao);
        }

        if (moduloId is null) return NotFound();
        var modulo = await _db.Modulos.FindAsync(moduloId);
        if (modulo == null) return NotFound();
        ViewBag.CursoId = modulo.CursoOnlineId;
        var proximaOrdem = await _db.Licoes.Where(l => l.ModuloId == moduloId).CountAsync() + 1;
        return View(new Licao { ModuloId = moduloId.Value, Ordem = proximaOrdem });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Licao(Licao model)
    {
        var modulo = await _db.Modulos.FindAsync(model.ModuloId);
        if (modulo == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.CursoId = modulo.CursoOnlineId;
            return View(model);
        }

        if (model.Id == 0)
        {
            _db.Licoes.Add(model);
        }
        else
        {
            var licao = await _db.Licoes.FindAsync(model.Id);
            if (licao == null) return NotFound();
            licao.Titulo = model.Titulo;
            licao.Descricao = model.Descricao;
            licao.Ordem = model.Ordem;
            licao.Duracao = model.Duracao;
            licao.UrlVideoVimeo = model.UrlVideoVimeo;
        }
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Aula salva.";
        return RedirectToAction(nameof(Conteudo), new { id = modulo.CursoOnlineId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLicao(int id)
    {
        var licao = await _db.Licoes.Include(l => l.Modulo).FirstOrDefaultAsync(l => l.Id == id);
        var cursoId = licao?.Modulo?.CursoOnlineId ?? 0;
        if (licao != null)
        {
            _db.Licoes.Remove(licao);
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Aula excluída.";
        }
        return RedirectToAction(nameof(Conteudo), new { id = cursoId });
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private async Task<string> GerarSlugUnicoAsync(string? slug, string? nome, int? idAtual)
    {
        var baseSlug = Slugify(!string.IsNullOrWhiteSpace(slug) ? slug : nome ?? "curso");
        var final = baseSlug;
        var i = 1;
        while (await _db.CursosOnline.AnyAsync(c => c.Slug == final && c.Id != idAtual))
            final = $"{baseSlug}-{++i}";
        return final;
    }

    private static string Slugify(string texto)
    {
        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        var s = sb.ToString().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s-]+", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? "curso" : s;
    }
}
