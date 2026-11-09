using Microsoft.AspNetCore.Mvc;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>
/// Hotsites de venda por curso (páginas estáticas em wwwroot/curso/{slug}/,
/// para divulgar no Instagram). Esta rota só deixa a URL limpa, sem a barra
/// final: /curso/{slug} → /curso/{slug}/ (onde o index.html estático é servido).
/// </summary>
public class HotsiteController : Controller
{
    [HttpGet("/curso/{slug}")]
    public IActionResult Index(string slug) => RedirectPermanent($"/curso/{slug}/");
}
