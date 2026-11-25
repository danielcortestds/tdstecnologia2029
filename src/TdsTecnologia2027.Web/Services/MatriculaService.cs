using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

public class ResultadoProvisao
{
    public Aluno Aluno { get; set; } = null!;
    public bool NovoUsuario { get; set; }
    public string? SenhaGerada { get; set; }
    public List<string> CursosLiberados { get; set; } = new();
}

/// <summary>
/// Cria/garante o aluno e libera o acesso aos cursos (matrícula vitalícia) ou a
/// assinatura (TDS Pass). Usado pelo pagamento (webhook/retorno) e pelo resgate
/// de vouchers. Idempotente: chamar de novo não duplica nem reseta senha.
/// </summary>
public class MatriculaService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<Aluno> _hasher;
    private readonly IEmailSender _email;
    private readonly ILogger<MatriculaService> _logger;

    public MatriculaService(
        ApplicationDbContext db,
        IPasswordHasher<Aluno> hasher,
        IEmailSender email,
        ILogger<MatriculaService> logger)
    {
        _db = db;
        _hasher = hasher;
        _email = email;
        _logger = logger;
    }

    public async Task<ResultadoProvisao> ProvisionarAsync(
        string nome, string email, string? telefone, IEnumerable<int> cursoOnlineIds, bool enviarEmail = true)
    {
        email = email.Trim().ToLowerInvariant();
        var ids = cursoOnlineIds.Distinct().ToList();
        var resultado = new ResultadoProvisao();

        var (aluno, senhaGerada, novo) = await GarantirAlunoAsync(nome, email, telefone);
        resultado.NovoUsuario = novo;
        resultado.SenhaGerada = senhaGerada;

        // Matrículas vitalícias (DataFim = null)
        foreach (var cursoId in ids)
        {
            var matricula = aluno.Matriculas.FirstOrDefault(m => m.CursoOnlineId == cursoId);
            if (matricula == null)
            {
                _db.Matriculas.Add(new MatriculaCurso
                {
                    AlunoId = aluno.Id,
                    CursoOnlineId = cursoId,
                    DataInicio = DateTime.Today,
                    DataFim = null,          // vitalício
                    Ativo = true
                });
            }
            else
            {
                matricula.Ativo = true;
                matricula.DataFim = null;
            }
        }
        await _db.SaveChangesAsync();

        resultado.Aluno = aluno;
        resultado.CursosLiberados = await _db.CursosOnline
            .Where(c => ids.Contains(c.Id))
            .Select(c => c.Nome)
            .ToListAsync();

        if (enviarEmail)
        {
            try
            {
                await _email.EnviarAcessoAsync(email, aluno.NomeCompleto, resultado.CursosLiberados, resultado.SenhaGerada);
            }
            catch (Exception ex)
            {
                // Não falha a liberação se o e-mail não sair — o acesso já está garantido.
                _logger.LogError(ex, "[TDS] Acesso liberado para {Email}, mas o e-mail falhou.", email);
            }
        }

        return resultado;
    }

    /// <summary>
    /// Cria/garante o aluno e a assinatura (TDS Pass), liberando acesso a todos os
    /// cursos até <c>DataFim</c>. Idempotente por <paramref name="pedidoId"/>.
    /// </summary>
    public async Task<Assinatura> ProvisionarAssinaturaAsync(
        string nome, string email, string? telefone, PlanoAssinatura plano, int? pedidoId, bool enviarEmail = true)
    {
        email = email.Trim().ToLowerInvariant();
        var (aluno, senhaGerada, _) = await GarantirAlunoAsync(nome, email, telefone);

        // Idempotência: se o pedido já gerou uma assinatura, reaproveita (webhook repetido).
        var assinatura = pedidoId.HasValue
            ? await _db.Assinaturas.FirstOrDefaultAsync(a => a.PedidoId == pedidoId.Value)
            : null;

        if (assinatura == null)
        {
            var inicio = DateTime.Today;
            assinatura = new Assinatura
            {
                AlunoId = aluno.Id,
                PlanoAssinaturaId = plano.Id,
                DataInicio = inicio,
                DataFim = inicio.AddMonths(plano.MesesTotais()),
                Status = StatusAssinatura.Ativa,
                PedidoId = pedidoId
            };
            _db.Assinaturas.Add(assinatura);
            await _db.SaveChangesAsync();

            if (enviarEmail)
            {
                try
                {
                    await _email.EnviarAcessoPassAsync(email, aluno.NomeCompleto, assinatura.DataFim, senhaGerada);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TDS] Assinatura criada para {Email}, mas o e-mail falhou.", email);
                }
            }
        }

        return assinatura;
    }

    /// <summary>Cria o aluno (com senha) se não existir, ou define senha se faltar.
    /// Retorna o aluno (com Matriculas carregadas), a senha gerada (quando houver) e se é novo.</summary>
    private async Task<(Aluno aluno, string? senhaGerada, bool novo)> GarantirAlunoAsync(
        string nome, string email, string? telefone)
    {
        email = email.Trim().ToLowerInvariant();
        var aluno = await _db.Alunos.Include(a => a.Matriculas)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email);

        if (aluno == null)
        {
            var senha = GerarSenha();
            aluno = new Aluno
            {
                NomeCompleto = string.IsNullOrWhiteSpace(nome) ? email : nome.Trim(),
                Email = email,
                Telefone = telefone,
                Status = StatusAluno.Ativo
            };
            aluno.SenhaHash = _hasher.HashPassword(aluno, senha);
            _db.Alunos.Add(aluno);
            await _db.SaveChangesAsync();
            aluno = await _db.Alunos.Include(a => a.Matriculas).FirstAsync(a => a.Id == aluno.Id);
            return (aluno, senha, true);
        }

        if (string.IsNullOrEmpty(aluno.SenhaHash))
        {
            // Aluno existia sem senha (ex.: migração) — define uma agora e informa.
            var senha = GerarSenha();
            aluno.SenhaHash = _hasher.HashPassword(aluno, senha);
            await _db.SaveChangesAsync();
            return (aluno, senha, false);
        }

        return (aluno, null, false);
    }

    /// <summary>Gera uma senha forte que atende às regras do Identity.</summary>
    private static string GerarSenha()
    {
        const string maius = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minus = "abcdefghijkmnpqrstuvwxyz";
        const string nums = "23456789";
        const string esp = "@#!$%";
        string P(string set, int n) => new(Enumerable.Range(0, n)
            .Select(_ => set[RandomNumberGenerator.GetInt32(set.Length)]).ToArray());
        // Ex: "Ab3xk7@p" — 1 maiúscula, minúsculas, 2 dígitos, 1 especial.
        return P(maius, 1) + P(minus, 4) + P(nums, 2) + P(esp, 1);
    }
}
