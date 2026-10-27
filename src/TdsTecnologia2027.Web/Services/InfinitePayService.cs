using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

public class ResultadoVerificacao
{
    public bool Pago { get; set; }
    public long ValorCentavos { get; set; }
    public string? MetodoPagamento { get; set; }
}

/// <summary>
/// Integração com o Checkout da InfinitePay:
///   • cria um link de pagamento hospedado (POST /links)
///   • confere o status de um pagamento (POST /payment_check)
/// Credenciais vêm da tabela ConfiguracaoLoja (editável no Admin).
/// Docs: https://www.infinitepay.io/checkout-documentacao
/// </summary>
public class InfinitePayService
{
    private const string BaseUrl = "https://api.checkout.infinitepay.io";
    private readonly IHttpClientFactory _httpFactory;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<InfinitePayService> _logger;

    public InfinitePayService(IHttpClientFactory httpFactory, ApplicationDbContext db, ILogger<InfinitePayService> logger)
    {
        _httpFactory = httpFactory;
        _db = db;
        _logger = logger;
    }

    public Task<ConfiguracaoLoja?> ConfiguracaoAsync() =>
        _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();

    /// <summary>Cria o link de pagamento e retorna a URL do checkout (ou null se falhar).</summary>
    public async Task<string?> CriarLinkAsync(Pedido pedido, string redirectUrl, string webhookUrl)
    {
        var cfg = await ConfiguracaoAsync();
        if (cfg is null || !cfg.EstaConfigurado)
            throw new InvalidOperationException("Pagamento online não está configurado (Admin → Loja).");

        var payload = new Dictionary<string, object?>
        {
            ["handle"] = cfg.Handle,
            ["order_nsu"] = pedido.OrderNsu,
            ["redirect_url"] = redirectUrl,
            ["webhook_url"] = webhookUrl,
            ["items"] = pedido.Itens.Select(i => new Dictionary<string, object?>
            {
                ["quantity"] = 1,
                ["price"] = (long)decimal.Round(i.Preco * 100m, 0),
                ["description"] = i.Descricao
            }).ToList(),
            ["customer"] = new Dictionary<string, object?>
            {
                ["name"] = pedido.NomeCliente,
                ["email"] = pedido.EmailCliente,
                ["phone_number"] = pedido.TelefoneCliente
            }
        };

        var json = await PostAsync("/links", payload, cfg.ApiKey);
        if (json is null) return null;

        // A resposta traz a URL do checkout (campo "url").
        if (json.Value.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String)
            return url.GetString();

        _logger.LogError("[TDS] InfinitePay /links sem campo 'url'. Resposta: {Resp}", json.Value.ToString());
        return null;
    }

    /// <summary>Confere o status do pagamento de um pedido no InfinitePay.</summary>
    public async Task<ResultadoVerificacao> VerificarAsync(string orderNsu, string? transactionNsu, string? slug)
    {
        var cfg = await ConfiguracaoAsync();
        if (cfg is null || string.IsNullOrWhiteSpace(cfg.Handle))
            return new ResultadoVerificacao { Pago = false };

        var payload = new Dictionary<string, object?>
        {
            ["handle"] = cfg.Handle,
            ["order_nsu"] = orderNsu,
            ["transaction_nsu"] = transactionNsu,
            ["slug"] = slug
        };

        var json = await PostAsync("/payment_check", payload, cfg.ApiKey);
        var r = new ResultadoVerificacao();
        if (json is null) return r;

        if (json.Value.TryGetProperty("paid", out var paid))
            r.Pago = paid.ValueKind == JsonValueKind.True;
        if (json.Value.TryGetProperty("amount", out var amount) && amount.TryGetInt64(out var cent))
            r.ValorCentavos = cent;
        if (json.Value.TryGetProperty("capture_method", out var cm) && cm.ValueKind == JsonValueKind.String)
            r.MetodoPagamento = cm.GetString();
        return r;
    }

    private async Task<JsonElement?> PostAsync(string caminho, object payload, string? apiKey)
    {
        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + caminho)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrWhiteSpace(apiKey))
                req.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);

            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("[TDS] InfinitePay {Caminho} HTTP {Code}: {Body}", caminho, (int)resp.StatusCode, body);
                return null;
            }
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TDS] Erro ao chamar InfinitePay {Caminho}.", caminho);
            return null;
        }
    }
}
