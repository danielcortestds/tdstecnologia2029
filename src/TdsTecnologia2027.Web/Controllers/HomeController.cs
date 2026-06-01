using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Carrega apenas cursos ativos, ordenados pelo campo Ordem (depois pelo Id como tiebreaker)
        var cursos = await _db.Cursos
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Id)
            .ToListAsync();

        return View(cursos);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
