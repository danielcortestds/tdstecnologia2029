using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class PagamentosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly MatriculaService _matricula;

    public PagamentosController(ApplicationDbContext db, MatriculaService matricula)
    {
        _db = db;
        _matricula = matricula;
    }

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

    // POST: Admin/Pagamentos/ConfirmarManual/5 — confirma um pedido à mão e libera o acesso.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarManual(int id)
    {
        var pedido = await _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null) return NotFound();

        if (pedido.Status != StatusPedido.Pago)
        {
            pedido.Status = StatusPedido.Pago;
            pedido.PagoEm = DateTime.Now;
            pedido.MetodoPagamento ??= "confirmado manualmente";
            await _db.SaveChangesAsync();

            var cursoIds = pedido.Itens.Select(i => i.CursoOnlineId).ToList();
            await _matricula.ProvisionarAsync(pedido.NomeCliente, pedido.EmailCliente, pedido.TelefoneCliente, cursoIds);
            TempData["Sucesso"] = $"Pedido confirmado e acesso liberado para {pedido.EmailCliente}.";
        }
        else
        {
            TempData["Sucesso"] = "Este pedido já estava pago.";
        }
        return RedirectToAction(nameof(Index));
    }
}
