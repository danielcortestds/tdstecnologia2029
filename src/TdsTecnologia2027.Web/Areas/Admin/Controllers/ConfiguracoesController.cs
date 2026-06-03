using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ConfiguracoesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;

    public ConfiguracoesController(ApplicationDbContext db, IEmailSender email)
    {
        _db = db;
        _email = email;
    }

    // GET: Admin/Configuracoes
    public async Task<IActionResult> Index()
    {
        var cfg = await _db.ConfiguracoesEmail.FirstOrDefaultAsync() ?? new ConfiguracaoEmail();
        return View(cfg);
    }

    // POST: Admin/Configuracoes/Salvar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(ConfiguracaoEmail model)
    {
        var cfg = await _db.ConfiguracoesEmail.FirstOrDefaultAsync();
        if (cfg is null)
        {
            cfg = new ConfiguracaoEmail();
            _db.ConfiguracoesEmail.Add(cfg);
        }

        cfg.ClientId = model.ClientId?.Trim();
        cfg.TenantId = model.TenantId?.Trim();
        // Mantém o secret atual se o campo vier vazio (evita apagar sem querer).
        if (!string.IsNullOrWhiteSpace(model.ClientSecret))
            cfg.ClientSecret = model.ClientSecret.Trim();
        cfg.EmailRemetente = model.EmailRemetente?.Trim();
        cfg.EmailDestino = model.EmailDestino?.Trim();
        cfg.Ativo = model.Ativo;

        await _db.SaveChangesAsync();
        TempData["Sucesso"] = "Configurações de e-mail salvas com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Configuracoes/Testar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Testar()
    {
        try
        {
            var cfg = await _db.ConfiguracoesEmail.AsNoTracking().FirstOrDefaultAsync();
            var destino = cfg?.EmailDestino ?? "(destino não configurado)";

            await _email.EnviarAsync(
                para: cfg?.EmailDestino ?? string.Empty,
                assunto: "Teste de envio — Site TDS Tecnologia",
                corpoHtml: "<h2>Funcionou! ✅</h2><p>Este é um e-mail de teste enviado pelo painel " +
                           "administrativo do site TDS Tecnologia via Microsoft Graph.</p>");

            TempData["Sucesso"] = $"E-mail de teste enviado para {destino}. Confira a caixa de entrada (e o spam).";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = "Falha ao enviar o e-mail de teste: " + (ex.InnerException?.Message ?? ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }
}
