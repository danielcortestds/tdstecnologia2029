using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

/// <summary>
/// TEMPORÁRIO — exporta a pasta de dados (banco SQLite + materiais) para download,
/// usado na migração. Remover após concluir a cópia dos dados.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Master")]
public class ExportController : Controller
{
    private readonly IWebHostEnvironment _env;

    public ExportController(IWebHostEnvironment env) => _env = env;

    // GET: /Admin/Export/Dados
    public IActionResult Dados()
    {
        var home = Environment.GetEnvironmentVariable("HOME");
        var dataDir = !string.IsNullOrWhiteSpace(home)
            ? Path.Combine(home, "data")
            : Path.Combine(_env.ContentRootPath, "App_Data");

        if (!Directory.Exists(dataDir))
            return NotFound("Pasta de dados não encontrada.");

        var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(dataDir, file).Replace('\\', '/');
                try
                {
                    var entry = zip.CreateEntry(rel, CompressionLevel.Optimal);
                    using var es = entry.Open();
                    // FileShare.ReadWrite permite copiar o .db mesmo com o app usando-o.
                    using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.CopyTo(es);
                }
                catch { /* arquivo em uso/efêmero — ignora */ }
            }
        }
        ms.Position = 0;
        return File(ms, "application/zip", $"tds-data-{DateTime.Now:yyyyMMdd-HHmm}.zip");
    }
}
