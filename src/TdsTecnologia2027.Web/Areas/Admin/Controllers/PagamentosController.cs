using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class PagamentosController : Controller
{
    private readonly ApplicationDbContext _db;

    public PagamentosController(ApplicationDbContext db) => _db = db;

    // GET: Admin/Pagamentos — configuração + últimos pedidos
    public async Task<IActionResult> Index()
    {
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync() ?? new ConfiguracaoLoja();
        ViewBag.Pedidos = await _db.Pedidos
            .Include(p => p.Itens)
            .OrderByDescending(p => p.CriadoEm)
            .Take(50)
            .ToListAsync();
        return View(cfg);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(ConfiguracaoLoja model)
    {
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync();
        if (cfg is null)
        {
            cfg = new ConfiguracaoLoja();
            _db.ConfiguracoesLoja.Add(cfg);
        }

        cfg.Handle = model.Handle?.Trim().TrimStart('$');
        // Mantém a API Key atual se o campo vier vazio (evita apagar sem querer).
        if (!string.IsNullOrWhiteSpace(model.ApiKey))
            cfg.ApiKey = model.ApiKey.Trim();
        cfg.Ativo = model.Ativo;

        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Configurações da loja salvas com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}
