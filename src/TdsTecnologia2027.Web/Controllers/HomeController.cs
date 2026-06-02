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
        const string msgSucesso = "Mensagem enviada com sucesso! Em breve entraremos em contato.";
        const string msgInvalido = "Por favor, preencha todos os campos corretamente.";
        const string msgErro = "Não foi possível enviar sua mensagem agora. " +
            "Tente novamente mais tarde ou fale conosco pelo WhatsApp.";

        // Honeypot anti-spam: se o campo invisível "Website" vier preenchido,
        // é um robô. Fingimos sucesso e descartamos a mensagem.
        if (!string.IsNullOrWhiteSpace(form.Website))
            return ResultadoContato(true, msgSucesso);

        if (!ModelState.IsValid)
            return ResultadoContato(false, msgInvalido);

        try
        {
            await _emailSender.EnviarContatoAsync(form);
            return ResultadoContato(true, msgSucesso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TDS] Falha ao enviar e-mail de contato.");
            return ResultadoContato(false, msgErro);
        }
    }

    /// <summary>
    /// Responde ao formulário de contato. Para envios AJAX (feito pelo template Canvas),
    /// devolve JSON no formato { alert, message } que o template espera. Para envios
    /// normais (sem JavaScript), faz redirect com mensagem em TempData.
    /// </summary>
    private IActionResult ResultadoContato(bool sucesso, string mensagem)
    {
        var ehAjax = string.Equals(
            Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (ehAjax)
            return Json(new { alert = sucesso ? "success" : "error", message = mensagem });

        TempData[sucesso ? "ContatoSucesso" : "ContatoErro"] = mensagem;
        return RedirectToAction(nameof(Index), "Home", null, "q-contact");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
