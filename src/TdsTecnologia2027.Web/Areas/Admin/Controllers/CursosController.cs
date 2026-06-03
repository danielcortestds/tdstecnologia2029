using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master,Editor")]
public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int TamanhoPagina = 10;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Cursos
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        ViewData["Busca"] = busca;

        var query = _context.Cursos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c => c.Titulo.Contains(busca));

        var total = await query.CountAsync();
        var cursos = await query
            // Ordena por data de início (mais próxima primeiro); cursos sem data vão ao final.
            .OrderBy(c => c.DataInicio == null)
            .ThenBy(c => c.DataInicio)
            .ThenBy(c => c.Id)
            .Skip((pagina - 1) * TamanhoPagina)
            .Take(TamanhoPagina)
            .ToListAsync();

        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)TamanhoPagina);
        ViewBag.Total = total;

        return View(cursos);
    }

    // GET: Admin/Cursos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        return View(curso);
    }

    // GET: Admin/Cursos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Cursos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Titulo,Slug,DescricaoCurta,Descricao,Categoria,Icone,CargaHoraria,Preco,ImagemUrl,DataInicio,PillSecundaria,IconePillSecundaria,Topicos,WhatsAppMensagem,Ordem,Ativo")] Curso curso)
    {
        // Garante slug único — devolve erro de validação no campo se já existir
        if (!string.IsNullOrWhiteSpace(curso.Slug) &&
            await _context.Cursos.AnyAsync(c => c.Slug == curso.Slug))
        {
            ModelState.AddModelError(nameof(curso.Slug), "Já existe um curso com este slug.");
        }

        if (ModelState.IsValid)
        {
            curso.CriadoEm = DateTime.Now;
            _context.Add(curso);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Curso criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    // GET: Admin/Cursos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        return View(curso);
    }

    // POST: Admin/Cursos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Slug,DescricaoCurta,Descricao,Categoria,Icone,CargaHoraria,Preco,ImagemUrl,DataInicio,PillSecundaria,IconePillSecundaria,Topicos,WhatsAppMensagem,Ordem,Ativo,CriadoEm")] Curso curso)
    {
        if (id != curso.Id) return NotFound();

        // Garante slug único (exceto o próprio registro)
        if (!string.IsNullOrWhiteSpace(curso.Slug) &&
            await _context.Cursos.AnyAsync(c => c.Slug == curso.Slug && c.Id != curso.Id))
        {
            ModelState.AddModelError(nameof(curso.Slug), "Já existe outro curso com este slug.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                curso.AtualizadoEm = DateTime.Now;
                _context.Update(curso);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Curso atualizado com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Cursos.AnyAsync(e => e.Id == curso.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    // GET: Admin/Cursos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        return View(curso);
    }

    // POST: Admin/Cursos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso != null)
        {
            // Soft delete: apenas desativa
            curso.Ativo = false;
            curso.AtualizadoEm = DateTime.Now;
            _context.Update(curso);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Curso desativado com sucesso!";
        }
        return RedirectToAction(nameof(Index));
    }
}
