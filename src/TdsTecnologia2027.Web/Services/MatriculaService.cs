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
/// Cria/garante o aluno e libera o acesso vitalício aos cursos. Usado tanto pelo
/// pagamento (webhook/retorno) quanto pelo resgate de vouchers. Idempotente:
/// chamar de novo para o mesmo aluno/curso não duplica nem reseta senha.
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

        // Aluno (tabela própria — autenticação separada do admin).
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
            resultado.NovoUsuario = true;
            resultado.SenhaGerada = senha;
        }
        else if (string.IsNullOrEmpty(aluno.SenhaHash))
        {
            // Aluno existia sem senha (ex.: migração) — define uma agora e informa.
            var senha = GerarSenha();
            aluno.SenhaHash = _hasher.HashPassword(aluno, senha);
            await _db.SaveChangesAsync();
            resultado.SenhaGerada = senha;
        }

        // 3) Matrículas vitalícias (DataFim = null)
        foreach (var cursoId in ids)
        {
            var matricula = aluno.Matriculas.FirstOrDefault(m => m.CursoOnlineId == cursoId);
            if (matricula == null)
            {
                matricula = new MatriculaCurso
                {
                    AlunoId = aluno.Id,
                    CursoOnlineId = cursoId,
                    DataInicio = DateTime.Today,
                    DataFim = null,          // vitalício
                    Ativo = true
                };
                _db.Matriculas.Add(matricula);
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

        // 4) E-mail de acesso
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
