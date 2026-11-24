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
    private readonly InfinitePayService _infinite;
    private readonly ILogger<PagamentosController> _logger;

    public PagamentosController(ApplicationDbContext db, MatriculaService matricula,
        InfinitePayService infinite, ILogger<PagamentosController> logger)
    {
        _db = db;
        _matricula = matricula;
        _infinite = infinite;
        _logger = logger;
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
            await LiberarAsync(pedido, pedido.MetodoPagamento ?? "confirmado manualmente");
            TempData["Sucesso"] = $"Pedido confirmado e acesso liberado para {pedido.EmailCliente}.";
        }
        else
        {
            TempData["Sucesso"] = "Este pedido já estava pago.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Pagamentos/ReconferirPendentes — consulta cada pedido pendente na
    // InfinitePay (payment_check) e libera automaticamente os que já estão pagos.
    // Rede de segurança para Pix em que o cliente não volta à página e o webhook falha,
    // e também diagnóstico: mostra o que a InfinitePay responde por pedido.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReconferirPendentes()
    {
        var pendentes = await _db.Pedidos.Include(p => p.Itens)
            .Where(p => p.Status == StatusPedido.Pendente)
            .OrderByDescending(p => p.CriadoEm)
            .Take(80)
            .ToListAsync();

        int confirmados = 0;
        var detalhes = new List<string>();
        foreach (var p in pendentes)
        {
            ResultadoVerificacao v;
            try
            {
                v = await _infinite.VerificarAsync(p.OrderNsu, p.TransactionNsu, p.InvoiceSlug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TDS] Reconferir: falha ao consultar pedido {Id}.", p.Id);
                detalhes.Add($"#{p.Id} {p.EmailCliente}: erro ao consultar");
                continue;
            }

            if (v.Pago)
            {
                var metodo = string.IsNullOrWhiteSpace(v.MetodoPagamento) ? "pix" : v.MetodoPagamento;
                await LiberarAsync(p, metodo!, v.ValorCentavos);
                confirmados++;
                var liq = v.ValorCentavos > 0 ? $" — recebido R$ {(v.ValorCentavos / 100m):N2}" : "";
                detalhes.Add($"#{p.Id} {p.EmailCliente}: PAGO{liq} → acesso liberado");
            }
            else
            {
                detalhes.Add($"#{p.Id} {p.EmailCliente}: ainda pendente na InfinitePay");
            }
        }

        TempData["Sucesso"] = confirmados > 0
            ? $"{confirmados} pedido(s) confirmado(s) e acesso liberado."
            : "Reconferência concluída. Nenhum pendente estava pago na InfinitePay.";
        TempData["ReconferirDetalhe"] = string.Join("\n", detalhes);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Marca o pedido como pago e libera/garante o acesso do aluno (idempotente).</summary>
    private async Task LiberarAsync(Pedido pedido, string metodo, long valorRecebidoCentavos = 0)
    {
        if (pedido.Status != StatusPedido.Pago)
        {
            pedido.Status = StatusPedido.Pago;
            pedido.PagoEm = DateTime.Now;
            pedido.MetodoPagamento = metodo;
            if (valorRecebidoCentavos > 0) pedido.ValorRecebido = valorRecebidoCentavos / 100m;
            await _db.SaveChangesAsync();
        }

        var cursoIds = pedido.Itens.Select(i => i.CursoOnlineId).ToList();
        await _matricula.ProvisionarAsync(pedido.NomeCliente, pedido.EmailCliente, pedido.TelefoneCliente, cursoIds);
    }
}
