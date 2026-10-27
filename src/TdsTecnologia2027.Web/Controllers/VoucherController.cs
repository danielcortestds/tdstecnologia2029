using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>
/// Resgate de voucher: o aluno informa o código + seus dados e o curso é
/// liberado na hora (acesso vitalício + e-mail com o login). Público.
/// </summary>
public class VoucherController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly MatriculaService _matricula;
    private readonly ILogger<VoucherController> _logger;

    public VoucherController(ApplicationDbContext db, MatriculaService matricula, ILogger<VoucherController> logger)
    {
        _db = db;
        _matricula = matricula;
        _logger = logger;
    }

    // GET: /Voucher/Resgatar?codigo=...
    [HttpGet]
    public IActionResult Resgatar(string? codigo)
        => View(new ResgateViewModel { Codigo = codigo?.Trim().ToUpperInvariant() ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resgatar(ResgateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var codigo = model.Codigo.Trim().ToUpperInvariant();
        var voucher = await _db.Vouchers
            .Include(v => v.CursoOnline)
            .FirstOrDefaultAsync(v => v.Codigo == codigo);

        if (voucher == null)
        {
            ModelState.AddModelError(nameof(model.Codigo), "Voucher não encontrado. Confira o código.");
            return View(model);
        }
        if (!voucher.Disponivel())
        {
            ModelState.AddModelError(nameof(model.Codigo), "Este voucher não está mais disponível (expirado ou já utilizado).");
            return View(model);
        }

        // Libera o acesso ao curso do voucher.
        var resultado = await _matricula.ProvisionarAsync(
            model.Nome, model.Email, null, new[] { voucher.CursoOnlineId });

        // Marca o uso + registra o resgate (auditoria).
        voucher.UsosFeitos += 1;
        _db.VouchersResgates.Add(new VoucherResgate
        {
            VoucherId = voucher.Id,
            Email = model.Email.Trim().ToLowerInvariant(),
            Nome = model.Nome.Trim(),
            AlunoId = resultado.Aluno.Id
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("[TDS] Voucher {Codigo} resgatado por {Email}.", codigo, model.Email);

        ViewBag.CursoNome = voucher.CursoOnline?.Nome;
        ViewBag.Email = model.Email.Trim().ToLowerInvariant();
        ViewBag.ContaNova = resultado.NovoUsuario;
        return View("ResgateOk");
    }
}
