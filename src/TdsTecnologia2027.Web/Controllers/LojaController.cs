using Microsoft.AspNetCore.Mvc;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>
/// Loja de cursos online: carrinho de compras (Fase 1) e, na Fase 2, checkout e
/// retorno do pagamento. Público.
/// </summary>
public class LojaController : Controller
{
    private readonly CarrinhoService _carrinho;

    public LojaController(CarrinhoService carrinho) => _carrinho = carrinho;

    /// <summary>Adiciona um curso ao carrinho. Responde JSON com o estado atual.</summary>
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
    public async Task<IActionResult> Limpar()
    {
        _carrinho.Limpar();
        return await EstadoJson(true, "Carrinho esvaziado.");
    }

    /// <summary>Estado atual do carrinho (para montar o drawer e o contador).</summary>
    [HttpGet]
    public async Task<IActionResult> Mini() => await EstadoJson(true, null);

    private async Task<IActionResult> EstadoJson(bool ok, string? mensagem)
    {
        var itens = await _carrinho.ItensAsync();
        return Json(new
        {
            ok,
            mensagem,
            quantidade = itens.Count,
            total = itens.Sum(i => i.Preco),
            totalFormatado = (await _carrinho.TotalAsync()).ToString("C", new System.Globalization.CultureInfo("pt-BR")),
            itens = itens.Select(i => new
            {
                id = i.CursoOnlineId,
                nome = i.Nome,
                preco = i.Preco,
                precoFormatado = i.Preco.ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                cor = string.IsNullOrWhiteSpace(i.CorCapa) ? "#0d9488" : i.CorCapa,
                icone = string.IsNullOrWhiteSpace(i.IconeCapa) ? "bi-play-circle" : i.IconeCapa
            })
        });
    }
}
