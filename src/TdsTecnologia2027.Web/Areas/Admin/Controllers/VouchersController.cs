using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class VouchersController : Controller
{
    private readonly ApplicationDbContext _db;

    public VouchersController(ApplicationDbContext db) => _db = db;

    // GET: Admin/Vouchers
    public async Task<IActionResult> Index()
    {
        var vouchers = await _db.Vouchers
            .Include(v => v.CursoOnline)
            .OrderByDescending(v => v.CriadoEm)
            .ToListAsync();
        return View(vouchers);
    }

    // GET: Admin/Vouchers/Create
    public async Task<IActionResult> Create()
    {
        await PopularCursosAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int cursoOnlineId, int quantidade = 1, int usosMaximos = 1,
        DateTime? validade = null, string? observacao = null)
    {
        var curso = await _db.CursosOnline.FindAsync(cursoOnlineId);
        if (curso == null)
        {
            ModelState.AddModelError(string.Empty, "Selecione um curso válido.");
            await PopularCursosAsync();
            return View();
        }

        quantidade = Math.Clamp(quantidade, 1, 500);
        usosMaximos = Math.Clamp(usosMaximos, 1, 100000);

        var gerados = new List<Voucher>();
        for (var i = 0; i < quantidade; i++)
        {
            var codigo = await GerarCodigoUnicoAsync();
            gerados.Add(new Voucher
            {
                Codigo = codigo,
                CursoOnlineId = cursoOnlineId,
                UsosMaximos = usosMaximos,
                Validade = validade,
                Observacao = observacao?.Trim(),
                Ativo = true
            });
        }
        _db.Vouchers.AddRange(gerados);
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = $"{quantidade} voucher(s) gerado(s) para o curso \"{curso.Nome}\".";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var v = await _db.Vouchers.FindAsync(id);
        if (v != null)
        {
            v.Ativo = !v.Ativo;
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = $"Voucher {v.Codigo} {(v.Ativo ? "ativado" : "desativado")}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private async Task PopularCursosAsync()
    {
        ViewBag.Cursos = new SelectList(
            await _db.CursosOnline.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome");
    }

    private async Task<string> GerarCodigoUnicoAsync()
    {
        // Evita caracteres ambíguos (0/O, 1/I).
        const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string Bloco(int n) => new(Enumerable.Range(0, n)
            .Select(_ => alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)]).ToArray());

        for (var tentativa = 0; tentativa < 20; tentativa++)
        {
            var codigo = $"TDS-{Bloco(4)}-{Bloco(4)}";
            if (!await _db.Vouchers.AnyAsync(v => v.Codigo == codigo))
                return codigo;
        }
        // Fallback improvável: acrescenta mais entropia.
        return $"TDS-{Bloco(4)}-{Bloco(4)}-{Bloco(2)}";
    }
}
