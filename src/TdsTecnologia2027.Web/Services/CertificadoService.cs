using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Regras e geração do certificado de conclusão: cálculo de progresso,
/// emissão (≥ 65%), validação por código e geração do PDF (com QR).
/// </summary>
public class CertificadoService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _http;

    public CertificadoService(ApplicationDbContext db, IWebHostEnvironment env, IHttpContextAccessor http)
    {
        _db = db;
        _env = env;
        _http = http;
    }

    /// <summary>Percentual de conclusão (aulas concluídas ÷ total de aulas).</summary>
    public async Task<int> PercentualAsync(int alunoId, int cursoOnlineId)
    {
        var licaoIds = await _db.Licoes
            .Where(l => l.Modulo!.CursoOnlineId == cursoOnlineId)
            .Select(l => l.Id)
            .ToListAsync();
        if (licaoIds.Count == 0) return 0;

        var concluidas = await _db.ProgressosLicao
            .CountAsync(p => p.AlunoId == alunoId && p.Concluida && licaoIds.Contains(p.LicaoId));

        return (int)Math.Round(concluidas * 100.0 / licaoIds.Count);
    }

    /// <summary>Retorna o certificado do aluno no curso, emitindo se atingiu o mínimo. Null se inelegível.</summary>
    public async Task<Certificado?> ObterOuEmitirAsync(Models.Aluno aluno, CursoOnline curso)
    {
        var existente = await _db.Certificados
            .FirstOrDefaultAsync(c => c.AlunoId == aluno.Id && c.CursoOnlineId == curso.Id);
        if (existente != null) return existente;

        var pct = await PercentualAsync(aluno.Id, curso.Id);
        if (pct < Certificado.PercentualMinimo) return null;

        var cert = new Certificado
        {
            AlunoId = aluno.Id,
            CursoOnlineId = curso.Id,
            CodigoValidacao = await GerarCodigoUnicoAsync(),
            NomeAluno = aluno.NomeCompleto,
            CursoNome = curso.Nome,
            CargaHoraria = curso.CargaHoraria,
            PeriodoExecucao = await CalcularPeriodoAsync(aluno.Id, curso.Id),
            PercentualNaEmissao = pct,
            EmitidoEm = DateTime.Now
        };
        _db.Certificados.Add(cert);
        await _db.SaveChangesAsync();
        return cert;
    }

    /// <summary>Período (mês/ano) em que o aluno realizou o curso, a partir dos acessos às aulas.</summary>
    private async Task<string> CalcularPeriodoAsync(int alunoId, int cursoOnlineId)
    {
        var pt = new System.Globalization.CultureInfo("pt-BR");
        var licaoIds = await _db.Licoes
            .Where(l => l.Modulo!.CursoOnlineId == cursoOnlineId).Select(l => l.Id).ToListAsync();

        var acessos = await _db.ProgressosLicao
            .Where(p => p.AlunoId == alunoId && licaoIds.Contains(p.LicaoId))
            .Select(p => p.UltimoAcesso)
            .ToListAsync();

        if (acessos.Count == 0)
            return DateTime.Now.ToString("MMMM 'de' yyyy", pt);

        var ini = acessos.Min();
        var fim = acessos.Max();
        string MesAno(DateTime d) => d.ToString("MMMM 'de' yyyy", pt);

        if (ini.Year == fim.Year && ini.Month == fim.Month)
            return MesAno(fim);
        if (ini.Year == fim.Year)
            return $"{ini.ToString("MMMM", pt)} a {MesAno(fim)}";
        return $"{MesAno(ini)} a {MesAno(fim)}";
    }

    public Task<Certificado?> ObterDoAlunoAsync(int alunoId, int cursoOnlineId) =>
        _db.Certificados.FirstOrDefaultAsync(c => c.AlunoId == alunoId && c.CursoOnlineId == cursoOnlineId);

    public Task<Certificado?> ObterPorCodigoAsync(string codigo) =>
        _db.Certificados.AsNoTracking().FirstOrDefaultAsync(c => c.CodigoValidacao == codigo);

    /// <summary>URL pública de validação do certificado.</summary>
    public string UrlValidacao(string codigo)
    {
        var req = _http.HttpContext?.Request;
        var baseUrl = req != null ? $"{req.Scheme}://{req.Host}" : "https://tdstecnologia.com.br";
        return $"{baseUrl}/Certificado/Validar/{codigo}";
    }

    // ── Geração do PDF ──────────────────────────────────────────────────────
    public byte[] GerarPdf(Certificado cert, CursoOnline curso)
    {
        var tema = Tema(curso.Fabricante);
        var logos = CarregarLogosTema(curso.Fabricante);
        var fundo = CarregarFundo(curso.Fabricante);
        var padrao = GerarPadraoSvg(tema);
        var assinatura = CarregarImagem("assinatura.png");
        var qr = GerarQrPng(UrlValidacao(cert.CodigoValidacao));
        var data = cert.EmitidoEm.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.0f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(13).FontColor("#334155").FontFamily("Lato"));

                // Moldura sólida (dupla) + marca d'água de padronagem preenchendo o quadro.
                page.Content()
                    .Border(5).BorderColor(tema.Primaria)
                    .Padding(3)
                    .Border(1.5f).BorderColor(tema.Primaria)
                    .Layers(layers =>
                    {
                        // Marca d'água: padronagem decorativa (degradê claro) preenchendo o quadro.
                        if (fundo != null)
                            layers.Layer().Image(fundo).FitArea();
                        else
                            layers.Layer().Svg(padrao);

                        layers.PrimaryLayer().PaddingVertical(fundo == null ? 22 : 36).PaddingHorizontal(fundo == null ? 30 : 40).Column(col =>
                        {
                        // Logos
                        if (logos.Count > 0)
                        {
                            col.Item().AlignCenter().Height(72).Row(row =>
                            {
                                row.Spacing(34);
                                foreach (var img in logos)
                                    row.AutoItem().Height(66).Image(img).FitHeight();
                            });
                        }

                        col.Item().PaddingTop(8).AlignCenter().Text("CERTIFICADO DE CONCLUSÃO")
                            .FontSize(28).Bold().FontColor(tema.Primaria).LetterSpacing(0.08f);

                        col.Item().PaddingTop(3).AlignCenter().Text("A TDS Tecnologia certifica que")
                            .FontSize(13).FontColor("#64748b");

                        col.Item().PaddingTop(10).AlignCenter().Text(cert.NomeAluno)
                            .FontSize(25).Bold().FontColor("#0f172a");

                        col.Item().PaddingTop(8).AlignCenter().MaxWidth(640).Text(txt =>
                        {
                            txt.AlignCenter();
                            txt.DefaultTextStyle(x => x.FontSize(14).FontColor("#334155").LineHeight(1.5f));
                            txt.Span("concluiu com êxito o curso ");
                            txt.Span(cert.CursoNome).Bold().FontColor("#0f172a");
                            txt.Span(".");
                        });

                        // Carga horária + período de realização
                        col.Item().PaddingTop(10).AlignCenter().Text(txt =>
                        {
                            txt.AlignCenter();
                            txt.DefaultTextStyle(x => x.FontSize(12).FontColor("#334155"));
                            if (cert.CargaHoraria > 0)
                            {
                                txt.Span("Carga horária: ").FontColor("#64748b");
                                txt.Span($"{cert.CargaHoraria} horas").Bold();
                            }
                            if (cert.CargaHoraria > 0 && !string.IsNullOrWhiteSpace(cert.PeriodoExecucao))
                                txt.Span("      ");
                            if (!string.IsNullOrWhiteSpace(cert.PeriodoExecucao))
                            {
                                txt.Span("Período de realização: ").FontColor("#64748b");
                                txt.Span(cert.PeriodoExecucao!).Bold();
                            }
                        });

                        col.Item().PaddingTop(10).AlignCenter().Text($"Emitido em {data}")
                            .FontSize(12).FontColor("#64748b");

                        // Assinatura
                        col.Item().PaddingTop(12).AlignCenter().Column(sig =>
                        {
                            if (assinatura != null)
                                sig.Item().AlignCenter().Height(52).Image(assinatura).FitHeight();
                            else
                                sig.Item().AlignCenter().PaddingBottom(0).Text("Daniel Côrtes")
                                    .FontFamily("Great Vibes").FontSize(32).FontColor("#1b3a8f");
                            sig.Item().Width(240).LineHorizontal(0.75f).LineColor("#94a3b8");
                            sig.Item().PaddingTop(5).AlignCenter().Text("TDS Tecnologia").FontSize(12).Bold().FontColor("#0f172a");
                            sig.Item().AlignCenter().Text("Treinamento e Consultoria em Informática").FontSize(9).FontColor("#94a3b8");
                            sig.Item().AlignCenter().Text("CNPJ 17.970.079/0001-86").FontSize(9).FontColor("#94a3b8");
                        });

                        // Rodapé: validação + QR
                        col.Item().PaddingTop(12).Row(row =>
                        {
                            row.RelativeItem().AlignBottom().Column(c =>
                            {
                                c.Item().Text("Validação do certificado").FontSize(8).Bold().FontColor("#94a3b8");
                                c.Item().Text(UrlValidacao(cert.CodigoValidacao)).FontSize(8).FontColor("#94a3b8");
                                c.Item().Text($"Código: {cert.CodigoValidacao}").FontSize(8).FontColor("#cbd5e1");
                            });
                            row.ConstantItem(56).Height(56).Image(qr).FitArea();
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private sealed record TemaCertificado(string Primaria, string Clara, string? ParceiroArquivo);

    /// <summary>Cores e logo de parceiro por fabricante. Todos emitidos pela TDS.</summary>
    private static TemaCertificado Tema(FabricanteCertificado fab) => fab switch
    {
        FabricanteCertificado.Qlik => new TemaCertificado("#009845", "#bbf7d0", "qlik-partner.png"),
        FabricanteCertificado.Microsoft => new TemaCertificado("#0067b8", "#cfe4fa", "microsoft-partner.png"),
        _ => new TemaCertificado("#15356a", "#c7d2fe", null) // TDS (azul-marinho da logo)
    };

    private static (int r, int g, int b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        return (Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16));
    }

    /// <summary>Padronagem decorativa (guilloché) em degradê bem claro, tingida na cor do tema.</summary>
    private static string GerarPadraoSvg(TemaCertificado tema)
    {
        var (r, g, b) = HexToRgb(tema.Primaria);
        // Clareia a cor misturando com branco (f = 0 cor cheia, 1 = branco).
        string Clarear(double f) =>
            $"#{(int)(r + (255 - r) * f):X2}{(int)(g + (255 - g) * f):X2}{(int)(b + (255 - b) * f):X2}";
        var tint = Clarear(0.80);

        return $$"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1414 1000" preserveAspectRatio="none">
              <defs>
                <linearGradient id="grad" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="{{tint}}" stop-opacity="0.85"/>
                  <stop offset="0.55" stop-color="{{Clarear(0.92)}}" stop-opacity="0.5"/>
                  <stop offset="1" stop-color="#ffffff" stop-opacity="0"/>
                </linearGradient>
                <pattern id="pat" width="150" height="150" patternUnits="userSpaceOnUse" patternTransform="rotate(8)">
                  <g stroke="{{tema.Primaria}}" stroke-width="1.1" fill="none" opacity="0.06">
                    <circle cx="38" cy="38" r="28"/>
                    <circle cx="38" cy="38" r="15"/>
                    <circle cx="113" cy="113" r="28"/>
                    <circle cx="113" cy="113" r="15"/>
                    <circle cx="113" cy="38" r="7"/>
                    <circle cx="38" cy="113" r="7"/>
                    <path d="M0 75 Q 75 42 150 75"/>
                    <path d="M75 0 Q 42 75 75 150"/>
                  </g>
                </pattern>
              </defs>
              <rect width="1414" height="1000" fill="#ffffff"/>
              <rect width="1414" height="1000" fill="url(#grad)"/>
              <rect width="1414" height="1000" fill="url(#pat)"/>
            </svg>
            """;
    }

    /// <summary>Logos do certificado: TDS sempre; logo de parceiro conforme o fabricante (se o arquivo existir).</summary>
    private List<byte[]> CarregarLogosTema(FabricanteCertificado fab)
    {
        var imagens = new List<byte[]>();
        var tds = CarregarImagem("tds.png");
        if (tds != null) imagens.Add(tds);

        var parceiro = Tema(fab).ParceiroArquivo;
        if (parceiro != null)
        {
            var img = CarregarImagem(parceiro);
            if (img != null) imagens.Add(img);
        }
        return imagens;
    }

    /// <summary>Arte de fundo opcional por fabricante (fundo-tds.png / fundo-qlik.png / fundo-microsoft.png).</summary>
    private byte[]? CarregarFundo(FabricanteCertificado fab)
        => CarregarImagem("fundo-" + fab.ToString().ToLowerInvariant() + ".png");

    private byte[]? CarregarImagem(string arquivo)
    {
        var caminho = Path.Combine(_env.WebRootPath, "images", "certificados", arquivo);
        if (!File.Exists(caminho)) return null;
        try { return File.ReadAllBytes(caminho); } catch { return null; }
    }

    private static byte[] GerarQrPng(string texto)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(10); // 10px por módulo; QuestPDF redimensiona
    }

    private async Task<string> GerarCodigoUnicoAsync()
    {
        const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string Bloco(int n) => new(Enumerable.Range(0, n)
            .Select(_ => alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)]).ToArray());

        for (var i = 0; i < 20; i++)
        {
            var codigo = $"{Bloco(4)}-{Bloco(4)}";
            if (!await _db.Certificados.AnyAsync(c => c.CodigoValidacao == codigo))
                return codigo;
        }
        return $"{Bloco(4)}-{Bloco(4)}-{Bloco(2)}";
    }
}
