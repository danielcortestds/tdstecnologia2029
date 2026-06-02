using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext db, IEmailSender emailSender, ILogger<HomeController> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Carrega apenas cursos ativos, ordenados pelo campo Ordem (depois pelo Id como tiebreaker)
        var cursos = await _db.Cursos
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Id)
            .ToListAsync();

        return View(cursos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contato(ContatoForm form)
    {
        // Honeypot anti-spam: se o campo invisível "Website" vier preenchido,
        // é um robô. Fingimos sucesso e descartamos a mensagem.
        if (!string.IsNullOrWhiteSpace(form.Website))
        {
            TempData["ContatoSucesso"] = "Mensagem enviada com sucesso! Em breve entraremos em contato.";
            return RedirectToAction(nameof(Index), "Home", null, "q-contact");
        }

        if (!ModelState.IsValid)
        {
            TempData["ContatoErro"] = "Por favor, preencha todos os campos corretamente.";
            return RedirectToAction(nameof(Index), "Home", null, "q-contact");
        }

        try
        {
            await _emailSender.EnviarContatoAsync(form);
            TempData["ContatoSucesso"] = "Mensagem enviada com sucesso! Em breve entraremos em contato.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TDS] Falha ao enviar e-mail de contato.");
            TempData["ContatoErro"] = "Não foi possível enviar sua mensagem agora. " +
                "Tente novamente mais tarde ou fale conosco pelo WhatsApp.";
        }

        return RedirectToAction(nameof(Index), "Home", null, "q-contact");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
