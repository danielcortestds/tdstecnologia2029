using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>
/// Hot site DINÂMICO do TDS Pass (padrão V2, tom preto): lê o plano ativo e os
/// cursos publicados para mostrar, num carrossel, tudo o que a assinatura inclui.
/// Rota literal /curso/tds-pass (tem precedência sobre o HotsiteController /curso/{slug}).
/// </summary>
public class PassController : Controller
{
    private readonly ApplicationDbContext _db;
    public PassController(ApplicationDbContext db) => _db = db;

    [HttpGet("/curso/tds-pass")]
    public async Task<IActionResult> Index()
    {
        var plano = await _db.Planos.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Slug == "tds-pass" && p.Ativo)
                    ?? await _db.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Ativo);
        if (plano == null) return Redirect("/#features");

        var cursos = await _db.CursosOnline.AsNoTracking()
            .Include(c => c.Modulos).ThenInclude(m => m.Licoes)
            .Where(c => c.Status == StatusCursoOnline.Publicado)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .ToListAsync();

        return View(new PassHotsiteViewModel { Plano = plano, Cursos = cursos });
    }
}
