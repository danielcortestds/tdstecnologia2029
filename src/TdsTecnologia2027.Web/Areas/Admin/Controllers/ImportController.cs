using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TdsTecnologia2027.Web.Data;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

/// <summary>
/// TEMPORÁRIO — importa a pasta de dados (banco SQLite + materiais) a partir de
/// um .zip enviado por upload, usado na migração para o novo App Service.
/// O banco é gravado como "&lt;banco&gt;.incoming" e trocado no próximo boot
/// (ver Program.cs) para evitar travas de arquivo. Remover após concluir.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Master")]
[IgnoreAntiforgeryToken]
public class ImportController : Controller
{
    private readonly IWebHostEnvironment _env;

    public ImportController(IWebHostEnvironment env) => _env = env;

    // GET: /Admin/Import/Dados — formulário de upload.
    [HttpGet]
    public IActionResult Dados() => Pagina(FormularioHtml());

    // POST: /Admin/Import/Dados — recebe o .zip e extrai para a pasta de dados.
    [HttpPost]
    [RequestSizeLimit(200_000_000)] // 200 MB
    public async Task<IActionResult> Dados(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return Pagina(MensagemHtml("erro", "Nenhum arquivo enviado.", voltar: true));

        var dataDir = ConnectionStringResolver.DataDir(_env);
        var dbPath = Path.Combine(dataDir, ConnectionStringResolver.DbFileName);

        int bancos = 0, materiais = 0;
        try
        {
            await using var upload = arquivo.OpenReadStream();
            using var zip = new ZipArchive(upload, ZipArchiveMode.Read);

            foreach (var entry in zip.Entries)
            {
                // Pula diretórios e arquivos auxiliares/temporários.
                if (string.IsNullOrEmpty(entry.Name)) continue;
                var nome = entry.Name;
                if (nome.EndsWith(".local-bak", StringComparison.OrdinalIgnoreCase)) continue;
                if (nome.EndsWith(".incoming", StringComparison.OrdinalIgnoreCase)) continue;
                if (nome.EndsWith("-wal", StringComparison.OrdinalIgnoreCase)) continue;
                if (nome.EndsWith("-shm", StringComparison.OrdinalIgnoreCase)) continue;

                var rel = entry.FullName.Replace('\\', '/');

                if (nome.Equals(ConnectionStringResolver.DbFileName, StringComparison.OrdinalIgnoreCase))
                {
                    // Banco → grava como .incoming (troca no próximo boot).
                    await ExtrairAsync(entry, dbPath + ".incoming");
                    bancos++;
                }
                else
                {
                    // Materiais e demais arquivos → grava direto na pasta de dados.
                    var destino = Path.Combine(dataDir, rel.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
                    await ExtrairAsync(entry, destino);
                    materiais++;
                }
            }
        }
        catch (Exception ex)
        {
            return Pagina(MensagemHtml("erro",
                $"Falha ao ler o arquivo .zip: {System.Net.WebUtility.HtmlEncode(ex.Message)}", voltar: true));
        }

        if (bancos == 0)
            return Pagina(MensagemHtml("erro",
                $"O .zip não continha o banco <b>{ConnectionStringResolver.DbFileName}</b>. " +
                "Confira se enviou o arquivo correto.", voltar: true));

        var msg = $"Dados recebidos com sucesso! Banco atualizado e <b>{materiais}</b> arquivo(s) de material importado(s)." +
                  "<br><br><b>Falta 1 passo:</b> no portal do Azure, no App Service <b>sitetds</b>, clique em " +
                  "<b>Reiniciar</b> (Restart). Após ~30&nbsp;segundos, recarregue o site e seus cursos, alunos e " +
                  "calendário estarão lá. 🎉";
        return Pagina(MensagemHtml("ok", msg, voltar: false));
    }

    private static async Task ExtrairAsync(ZipArchiveEntry entry, string destino)
    {
        await using var es = entry.Open();
        await using var fs = new FileStream(destino, FileMode.Create, FileAccess.Write, FileShare.None);
        await es.CopyToAsync(fs);
    }

    // ── HTML (página autossuficiente, sem view) ───────────────────────────
    private ContentResult Pagina(string corpo) => Content(Shell(corpo), "text/html; charset=utf-8");

    private static string FormularioHtml() => """
        <h1>Importar dados</h1>
        <p class="sub">Envie o arquivo <b>.zip</b> gerado na máquina (banco + materiais).
        O banco atual será substituído pelos dados do arquivo.</p>
        <form method="post" enctype="multipart/form-data">
            <input type="file" name="arquivo" accept=".zip" required />
            <button type="submit">Enviar e importar</button>
        </form>
        """;

    private static string MensagemHtml(string tipo, string texto, bool voltar)
    {
        var cor = tipo == "ok" ? "#14b8a6" : "#f87171";
        var icone = tipo == "ok" ? "✓" : "!";
        var btn = voltar
            ? "<a class=\"btn\" href=\"/Admin/Import/Dados\">Tentar de novo</a>"
            : "<a class=\"btn\" href=\"/Admin\">Ir para o painel</a>";
        return $"""
            <div class="box" style="border-color:{cor}">
              <div class="ic" style="background:{cor}">{icone}</div>
              <p>{texto}</p>
            </div>
            {btn}
            """;
    }

    private static string Shell(string corpo) => $$"""
        <!DOCTYPE html>
        <html lang="pt-BR"><head><meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>Importar dados — TDS</title>
        <style>
          body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;
            font-family:system-ui,"Segoe UI",Roboto,sans-serif;background:#0b1220;color:#e2e8f0;padding:24px}
          .card{width:100%;max-width:520px;background:#0f172a;border:1px solid rgba(148,163,184,.15);
            border-radius:16px;padding:36px}
          h1{color:#f1f5f9;font-size:1.4rem;margin:0 0 6px}
          .sub{color:#94a3b8;font-size:.92rem;margin:0 0 22px}
          input[type=file]{width:100%;padding:14px;border:1px dashed rgba(148,163,184,.4);border-radius:10px;
            background:#0b1220;color:#cbd5e1;margin-bottom:18px}
          button,.btn{display:inline-block;background:#14b8a6;color:#04201c;font-weight:700;border:none;
            padding:13px 22px;border-radius:999px;cursor:pointer;font-size:.95rem;text-decoration:none}
          button:hover,.btn:hover{background:#0d9488;color:#fff}
          .box{display:flex;gap:14px;align-items:flex-start;border:1px solid;border-radius:12px;
            padding:18px;margin-bottom:20px;background:#0b1220}
          .box p{margin:0;line-height:1.5;color:#e2e8f0}
          .ic{flex:0 0 32px;width:32px;height:32px;border-radius:50%;color:#04201c;font-weight:800;
            display:flex;align-items:center;justify-content:center}
          b{color:#5eead4}
        </style></head>
        <body><main class="card">{{corpo}}</main></body></html>
        """;
}
