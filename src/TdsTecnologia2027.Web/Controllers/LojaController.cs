using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>
/// Loja de cursos online: carrinho, checkout (InfinitePay), retorno e webhook
/// de confirmação de pagamento (com liberação automática de acesso). Público.
/// </summary>
public class LojaController : Controller
{
    private readonly CarrinhoService _carrinho;
    private readonly InfinitePayService _infinite;
    private readonly MatriculaService _matricula;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<LojaController> _logger;

    public LojaController(
        CarrinhoService carrinho, InfinitePayService infinite, MatriculaService matricula,
        ApplicationDbContext db, ILogger<LojaController> logger)
    {
        _carrinho = carrinho;
        _infinite = infinite;
        _matricula = matricula;
        _db = db;
        _logger = logger;
    }

    // ── Carrinho (AJAX) ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Adicionar(int id)
    {
        var ok = await _carrinho.AdicionarAsync(id);
        return await EstadoJson(ok, ok ? "Curso adicionado ao carrinho." : "Curso indisponível.");
    }

    [HttpPost]
    public async Task<IActionResult> Remover(int id)
    {
        _carrinho.Remover(id);
        return await EstadoJson(true, "Removido do carrinho.");
    }

    [HttpPost]
    public async Task<IActionResult> Limpar() => await EstadoJson(true, "Carrinho esvaziado.");

    [HttpGet]
    public async Task<IActionResult> Mini() => await EstadoJson(true, null);

    private async Task<IActionResult> EstadoJson(bool ok, string? mensagem)
    {
        var itens = await _carrinho.ItensAsync();
        var cultura = new System.Globalization.CultureInfo("pt-BR");
        return Json(new
        {
            ok,
            mensagem,
            quantidade = itens.Count,
            total = itens.Sum(i => i.Preco),
            totalFormatado = itens.Sum(i => i.Preco).ToString("C", cultura),
            itens = itens.Select(i => new
            {
                id = i.CursoOnlineId,
                nome = i.Nome,
                preco = i.Preco,
                precoFormatado = i.Preco.ToString("C", cultura),
                cor = string.IsNullOrWhiteSpace(i.CorCapa) ? "#0d9488" : i.CorCapa,
                icone = string.IsNullOrWhiteSpace(i.IconeCapa) ? "bi-play-circle" : i.IconeCapa
            })
        });
    }

    // ── Checkout ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var itens = await _carrinho.ItensAsync();
        if (itens.Count == 0)
        {
            TempData["LojaErro"] = "Seu carrinho está vazio.";
            return Redirect("/#features");
        }

        var vm = new CheckoutViewModel { Itens = itens, Total = itens.Sum(i => i.Preco) };
        if (User.Identity?.IsAuthenticated == true)
            vm.Email = User.Identity.Name ?? string.Empty;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var itens = await _carrinho.ItensAsync();
        model.Itens = itens;
        model.Total = itens.Sum(i => i.Preco);

        if (itens.Count == 0)
        {
            TempData["LojaErro"] = "Seu carrinho está vazio.";
            return Redirect("/#features");
        }
        if (!ModelState.IsValid) return View(model);

        // Monta e salva o pedido (Pendente).
        var pedido = new Pedido
        {
            NomeCliente = model.Nome.Trim(),
            EmailCliente = model.Email.Trim().ToLowerInvariant(),
            TelefoneCliente = model.Telefone?.Trim(),
            Total = model.Total,
            Status = StatusPedido.Pendente,
            Itens = itens.Select(i => new PedidoItem
            {
                CursoOnlineId = i.CursoOnlineId,
                Descricao = i.Nome,
                Preco = i.Preco
            }).ToList()
        };
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        var redirectUrl = Url.Action(nameof(Retorno), "Loja", null, Request.Scheme)!;
        var webhookUrl = Url.Action(nameof(Webhook), "Loja", null, Request.Scheme)!;

        string? url;
        try
        {
            url = await _infinite.CriarLinkAsync(pedido, redirectUrl, webhookUrl);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[TDS] Checkout: pagamento não configurado.");
            ViewBag.Erro = "O pagamento online ainda não está disponível. Fale conosco pelo WhatsApp.";
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            ViewBag.Erro = "Não foi possível iniciar o pagamento agora. Tente novamente em instantes.";
            return View(model);
        }

        _carrinho.Limpar();
        return Redirect(url);
    }

    // ── Retorno do cliente (redirect_url do InfinitePay) ───────────────────
    [HttpGet]
    public async Task<IActionResult> Retorno(string? order_nsu, string? slug, string? transaction_nsu)
    {
        var pedido = string.IsNullOrWhiteSpace(order_nsu)
            ? null
            : await _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.OrderNsu == order_nsu);

        if (pedido == null)
        {
            ViewBag.Estado = "erro";
            return View();
        }

        if (pedido.Status != StatusPedido.Pago)
        {
            // Fallback: confere direto na InfinitePay (caso o webhook ainda não tenha chegado).
            var v = await _infinite.VerificarAsync(pedido.OrderNsu, transaction_nsu, slug);
            if (v.Pago && v.ValorCentavos >= pedido.TotalCentavos)
                await ConfirmarPedidoAsync(pedido, transaction_nsu, slug, v.MetodoPagamento);
        }

        ViewBag.Estado = pedido.Status == StatusPedido.Pago ? "pago" : "processando";
        ViewBag.Email = pedido.EmailCliente;
        return View(pedido);
    }

    // ── Webhook de pagamento aprovado (InfinitePay → nós) ──────────────────
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Webhook()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
            body = await reader.ReadToEndAsync();

        string? orderNsu = null, transactionNsu = null, slug = null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("order_nsu", out var o)) orderNsu = o.GetString();
            if (root.TryGetProperty("transaction_nsu", out var t)) transactionNsu = t.GetString();
            if (root.TryGetProperty("invoice_slug", out var s)) slug = s.GetString();
        }
        catch
        {
            _logger.LogWarning("[TDS] Webhook InfinitePay com corpo inválido: {Body}", body);
            return Ok(); // não pedir reenvio de algo que não dá para processar
        }

        if (string.IsNullOrWhiteSpace(orderNsu)) return Ok();

        var pedido = await _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.OrderNsu == orderNsu);
        if (pedido == null || pedido.Status == StatusPedido.Pago) return Ok(); // desconhecido ou já processado

        // Sem assinatura no webhook → confirmamos SEMPRE consultando o InfinitePay.
        var v = await _infinite.VerificarAsync(orderNsu, transactionNsu, slug);
        if (v.Pago && v.ValorCentavos >= pedido.TotalCentavos)
            await ConfirmarPedidoAsync(pedido, transactionNsu, slug, v.MetodoPagamento);
        else
            _logger.LogWarning("[TDS] Webhook: pagamento não confirmado para pedido {Nsu}.", orderNsu);

        return Ok();
    }

    /// <summary>Marca o pedido como pago e libera o acesso (idempotente).</summary>
    private async Task ConfirmarPedidoAsync(Pedido pedido, string? transactionNsu, string? slug, string? metodo)
    {
        if (pedido.Status == StatusPedido.Pago) return;

        pedido.Status = StatusPedido.Pago;
        pedido.PagoEm = DateTime.Now;
        pedido.TransactionNsu = transactionNsu;
        pedido.InvoiceSlug = slug;
        pedido.MetodoPagamento = metodo;
        await _db.SaveChangesAsync();

        var cursoIds = pedido.Itens.Select(i => i.CursoOnlineId).ToList();
        await _matricula.ProvisionarAsync(pedido.NomeCliente, pedido.EmailCliente, pedido.TelefoneCliente, cursoIds);
        _logger.LogInformation("[TDS] Pedido {Nsu} pago e acesso liberado para {Email}.", pedido.OrderNsu, pedido.EmailCliente);
    }
}
