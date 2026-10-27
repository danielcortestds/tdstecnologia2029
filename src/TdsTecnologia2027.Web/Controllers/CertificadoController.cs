using Microsoft.AspNetCore.Mvc;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Controllers;

/// <summary>Validação pública de certificados (destino do QR code). Público.</summary>
public class CertificadoController : Controller
{
    private readonly CertificadoService _cert;

    public CertificadoController(CertificadoService cert) => _cert = cert;

    // GET: /Certificado/Validar/{id}  (id = código de validação)
    [HttpGet]
    public async Task<IActionResult> Validar(string? id)
    {
        var codigo = id?.Trim().ToUpperInvariant();
        var cert = string.IsNullOrWhiteSpace(codigo) ? null : await _cert.ObterPorCodigoAsync(codigo);
        ViewBag.Codigo = codigo;
        return View(cert);
    }
}
