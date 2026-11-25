using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class AssinaturasController : Controller
{
    private readonly ApplicationDbContext _db;
    public AssinaturasController(ApplicationDbContext db) => _db = db;

    // GET: Admin/Assinaturas — configuração do plano + lista de assinaturas
    public async Task<IActionResult> Index()
    {
        var plano = await _db.Planos.FirstOrDefaultAsync(p => p.Slug == "tds-pass")
                    ?? await _db.Planos.FirstOrDefaultAsync();

        ViewBag.Assinaturas = await _db.Assinaturas
            .Include(a => a.Aluno)
            .OrderByDescending(a => a.CriadoEm)
            .Take(200)
            .ToListAsync();

        return View(plano);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarPlano(PlanoAssinatura model)
    {
        var plano = await _db.Planos.FirstOrDefaultAsync(p => p.Id == model.Id);
        if (plano == null)
        {
            TempData["AssErro"] = "Plano não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        plano.Nome = string.IsNullOrWhiteSpace(model.Nome) ? plano.Nome : model.Nome.Trim();
        plano.Descricao = model.Descricao;
        plano.Preco = model.Preco;
        plano.PrecoDe = model.PrecoDe;
        plano.DuracaoMeses = model.DuracaoMeses < 1 ? 1 : model.DuracaoMeses;
        plano.BonusMeses = model.BonusMeses < 0 ? 0 : model.BonusMeses;
        plano.BonusAtivo = model.BonusAtivo;
        plano.Ativo = model.Ativo;
        plano.AtualizadoEm = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["AssOk"] = "Plano salvo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Estender(int id, int meses)
    {
        var a = await _db.Assinaturas.FindAsync(id);
        if (a != null)
        {
            // Estende a partir de hoje se já venceu, senão a partir do fim atual.
            var baseData = a.DataFim.Date < DateTime.Today ? DateTime.Today : a.DataFim.Date;
            a.DataFim = baseData.AddMonths(meses <= 0 ? 6 : meses);
            if (a.Status == StatusAssinatura.Expirada) a.Status = StatusAssinatura.Ativa;
            await _db.SaveChangesAsync();
            TempData["AssOk"] = $"Assinatura estendida até {a.DataFim:dd/MM/yyyy}.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var a = await _db.Assinaturas.FindAsync(id);
        if (a != null)
        {
            a.Status = StatusAssinatura.Cancelada;
            await _db.SaveChangesAsync();
            TempData["AssOk"] = "Assinatura cancelada.";
        }
        return RedirectToAction(nameof(Index));
    }
}
