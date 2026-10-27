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
        var logos = CarregarLogos(curso.LogosCertificado);
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

                page.Content().Border(1.5f).BorderColor("#0d9488").Padding(6).Column(outer =>
                {
                    outer.Item().Border(0.75f).BorderColor("#99f6e4").Padding(26).Column(col =>
                    {
                        // Logos
                        if (logos.Count > 0)
                        {
                            col.Item().AlignCenter().Height(52).Row(row =>
                            {
                                row.Spacing(28);
                                foreach (var img in logos)
                                    row.AutoItem().Height(48).Image(img).FitHeight();
                            });
                        }

                        col.Item().PaddingTop(18).AlignCenter().Text("CERTIFICADO DE CONCLUSÃO")
                            .FontSize(30).Bold().FontColor("#0d9488").LetterSpacing(0.08f);

                        col.Item().PaddingTop(4).AlignCenter().Text("A TDS Tecnologia certifica que")
                            .FontSize(13).FontColor("#64748b");

                        col.Item().PaddingTop(14).AlignCenter().Text(cert.NomeAluno)
                            .FontSize(26).Bold().FontColor("#0f172a");

                        col.Item().PaddingTop(12).AlignCenter().MaxWidth(640).Text(txt =>
                        {
                            txt.AlignCenter();
                            txt.DefaultTextStyle(x => x.FontSize(14).FontColor("#334155").LineHeight(1.5f));
                            txt.Span("concluiu com êxito o curso ");
                            txt.Span(cert.CursoNome).Bold().FontColor("#0f172a");
                            txt.Span(".");
                        });

                        // Carga horária + período de realização
                        col.Item().PaddingTop(16).AlignCenter().Text(txt =>
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

                        col.Item().PaddingTop(22).AlignCenter().Text($"Emitido em {data}")
                            .FontSize(12).FontColor("#64748b");

                        // Assinatura
                        col.Item().PaddingTop(34).AlignCenter().Column(sig =>
                        {
                            sig.Item().Width(240).LineHorizontal(0.75f).LineColor("#94a3b8");
                            sig.Item().PaddingTop(5).AlignCenter().Text("TDS Tecnologia").FontSize(12).Bold().FontColor("#0f172a");
                            sig.Item().AlignCenter().Text("Treinamento e Consultoria em Informática").FontSize(9).FontColor("#94a3b8");
                            sig.Item().AlignCenter().Text("CNPJ 17.970.079/0001-86").FontSize(9).FontColor("#94a3b8");
                        });

                        // Rodapé: validação + QR
                        col.Item().PaddingTop(22).Row(row =>
                        {
                            row.RelativeItem().AlignBottom().Column(c =>
                            {
                                c.Item().Text("Validação do certificado").FontSize(8).Bold().FontColor("#94a3b8");
                                c.Item().Text(UrlValidacao(cert.CodigoValidacao)).FontSize(8).FontColor("#94a3b8");
                                c.Item().Text($"Código: {cert.CodigoValidacao}").FontSize(8).FontColor("#cbd5e1");
                            });
                            row.ConstantItem(62).Height(62).Image(qr).FitArea();
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private List<byte[]> CarregarLogos(string? csv)
    {
        var chaves = (csv ?? "tds")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLowerInvariant())
            .ToList();
        if (!chaves.Contains("tds")) chaves.Insert(0, "tds"); // TDS sempre
        chaves = chaves.Distinct().ToList();

        var pasta = Path.Combine(_env.WebRootPath, "images", "certificados");
        var imagens = new List<byte[]>();
        foreach (var chave in chaves)
        {
            var caminho = Path.Combine(pasta, chave + ".png");
            if (File.Exists(caminho))
            {
                try { imagens.Add(File.ReadAllBytes(caminho)); } catch { /* ignora logo inválido */ }
            }
        }
        return imagens;
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
