using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

/// <summary>
/// Login dos ALUNOS — ambiente próprio, separado da administração.
/// Autentica contra a tabela Alunos (SenhaHash) usando o cookie "Aluno".
/// </summary>
[Area("Aluno")]
[AllowAnonymous]
public class AcessoController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<Models.Aluno> _hasher;
    private readonly IEmailSender _email;
    private readonly ILogger<AcessoController> _logger;

    public AcessoController(ApplicationDbContext db, IPasswordHasher<Models.Aluno> hasher,
        IEmailSender email, ILogger<AcessoController> logger)
    {
        _db = db;
        _hasher = hasher;
        _email = email;
        _logger = logger;
    }

    // ── Login ───────────────────────────────────────────────────────────────
    [HttpGet("/Aluno/Entrar")]
    public IActionResult Entrar(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginAlunoViewModel());
    }

    [HttpPost("/Aluno/Entrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(LoginAlunoViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Email.ToLower() == email);

        // Mensagem genérica para não revelar se o e-mail existe.
        const string invalido = "E-mail ou senha incorretos.";

        if (aluno == null || string.IsNullOrEmpty(aluno.SenhaHash))
        {
            _logger.LogWarning("[TDS] Login de aluno falhou (conta inexistente ou sem senha): {Email}", email);
            ModelState.AddModelError(string.Empty, invalido);
            return View(model);
        }

        var verif = _hasher.VerifyHashedPassword(aluno, aluno.SenhaHash, model.Senha);
        if (verif == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, invalido);
            return View(model);
        }

        if (verif == PasswordVerificationResult.SuccessRehashNeeded)
        {
            aluno.SenhaHash = _hasher.HashPassword(aluno, model.Senha);
            await _db.SaveChangesAsync();
        }

        if (aluno.Status != StatusAluno.Ativo)
        {
            ModelState.AddModelError(string.Empty,
                "Seu acesso está inativo ou bloqueado. Fale com a TDS Tecnologia.");
            return View(model);
        }

        await AlunoAuth.EntrarAsync(HttpContext, aluno, model.Lembrar);
        return RedirecionarSeguro(returnUrl);
    }

    [HttpPost("/Aluno/Sair")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await AlunoAuth.SairAsync(HttpContext);
        return Redirect("/Aluno/Entrar");
    }

    // ── Esqueci minha senha ───────────────────────────────────────────────────
    [HttpGet("/Aluno/EsqueciSenha")]
    public IActionResult EsqueciSenha() => View(new EsqueciSenhaAlunoViewModel());

    [HttpPost("/Aluno/EsqueciSenha")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaAlunoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Enviado = true; // mostra a mesma confirmação sempre
        var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Email.ToLower() == email);
        if (aluno != null)
        {
            try
            {
                var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                aluno.TokenReset = token;
                aluno.TokenResetExpira = DateTime.UtcNow.AddHours(2);
                await _db.SaveChangesAsync();

                var link = Url.Action(nameof(RedefinirSenha), "Acesso",
                    new { area = "Aluno", token, email = aluno.Email }, Request.Scheme)!;
                var html = $@"<p>Olá,</p>
<p>Recebemos um pedido para redefinir a senha da sua conta de aluno na <strong>TDS Tecnologia</strong>.</p>
<p><a href=""{link}"" style=""background:#14b8a6;color:#04201c;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:700;display:inline-block;"">Criar nova senha</a></p>
<p>Se o botão não funcionar, copie e cole no navegador:<br><a href=""{link}"">{link}</a></p>
<p style=""color:#64748b;font-size:13px;"">O link vale por 2 horas. Se você não pediu isso, ignore este e-mail.</p>";
                await _email.EnviarAsync(aluno.Email, "Redefinição de senha — Área do Aluno TDS", html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TDS] Falha ao enviar reset de senha de aluno para {Email}.", email);
            }
        }
        return View(model);
    }

    // ── Redefinir senha (via link do e-mail) ──────────────────────────────────
    [HttpGet("/Aluno/RedefinirSenha")]
    public IActionResult RedefinirSenha(string? token, string? email)
        => View(new RedefinirSenhaAlunoViewModel { Token = token ?? "", Email = email ?? "" });

    [HttpPost("/Aluno/RedefinirSenha")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaAlunoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.Email.ToLower() == email);
        if (aluno == null || string.IsNullOrEmpty(aluno.TokenReset) ||
            aluno.TokenReset != model.Token ||
            aluno.TokenResetExpira == null || aluno.TokenResetExpira < DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Link inválido ou expirado. Peça um novo em 'Esqueci minha senha'.");
            return View(model);
        }

        aluno.SenhaHash = _hasher.HashPassword(aluno, model.NovaSenha);
        aluno.TokenReset = null;
        aluno.TokenResetExpira = null;
        await _db.SaveChangesAsync();

        model.Concluido = true;
        return View(model);
    }

    private IActionResult RedirecionarSeguro(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return Redirect("/Aluno");
    }
}

// ── ViewModels ────────────────────────────────────────────────────────────────
public class LoginAlunoViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Continuar conectado")]
    public bool Lembrar { get; set; }
}

public class EsqueciSenhaAlunoViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    public bool Enviado { get; set; }
}

public class RedefinirSenhaAlunoViewModel
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem.")]
    public string Confirmar { get; set; } = string.Empty;

    public bool Concluido { get; set; }
}
