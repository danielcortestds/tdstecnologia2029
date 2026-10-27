using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

/// <summary>Um item do carrinho, já resolvido com nome e preço do curso.</summary>
public class ItemCarrinho
{
    public int CursoOnlineId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string? CorCapa { get; set; }
    public string? IconeCapa { get; set; }
}

/// <summary>
/// Carrinho de compras guardado na sessão do visitante (lista de ids de cursos
/// online). Cada curso entra uma única vez (compra de acesso, não tem quantidade).
/// </summary>
public class CarrinhoService
{
    private const string ChaveSessao = "carrinho_cursos";
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;

    public CarrinhoService(IHttpContextAccessor http, ApplicationDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ISession Sessao => _http.HttpContext!.Session;

    private List<int> LerIds()
    {
        var json = Sessao.GetString(ChaveSessao);
        if (string.IsNullOrEmpty(json)) return new List<int>();
        try { return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>(); }
        catch { return new List<int>(); }
    }

    private void GravarIds(List<int> ids) =>
        Sessao.SetString(ChaveSessao, JsonSerializer.Serialize(ids));

    public List<int> Ids() => LerIds();

    public int Quantidade() => LerIds().Count;

    /// <summary>Adiciona um curso (se existir, estiver à venda e ainda não estiver no carrinho).</summary>
    public async Task<bool> AdicionarAsync(int cursoOnlineId)
    {
        var existe = await _db.CursosOnline
            .AsNoTracking()
            .AnyAsync(c => c.Id == cursoOnlineId && c.AVenda && c.Status == StatusCursoOnline.Publicado);
        if (!existe) return false;

        var ids = LerIds();
        if (!ids.Contains(cursoOnlineId))
        {
            ids.Add(cursoOnlineId);
            GravarIds(ids);
        }
        return true;
    }

    public void Remover(int cursoOnlineId)
    {
        var ids = LerIds();
        if (ids.Remove(cursoOnlineId)) GravarIds(ids);
    }

    public void Limpar() => Sessao.Remove(ChaveSessao);

    /// <summary>Resolve os itens do carrinho com nome/preço atuais do banco.</summary>
    public async Task<List<ItemCarrinho>> ItensAsync()
    {
        var ids = LerIds();
        if (ids.Count == 0) return new List<ItemCarrinho>();

        var cursos = await _db.CursosOnline
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new ItemCarrinho
            {
                CursoOnlineId = c.Id,
                Nome = c.Nome,
                Preco = c.Preco,
                CorCapa = c.CorCapa,
                IconeCapa = c.IconeCapa
            })
            .ToListAsync();

        // Preserva a ordem de inserção no carrinho.
        return ids
            .Select(id => cursos.FirstOrDefault(c => c.CursoOnlineId == id))
            .Where(c => c != null)
            .Cast<ItemCarrinho>()
            .ToList();
    }

    public async Task<decimal> TotalAsync() => (await ItensAsync()).Sum(i => i.Preco);
}
