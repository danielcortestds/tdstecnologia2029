using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

/// <summary>Conta do aluno: troca de senha (na tabela Alunos, ambiente próprio).</summary>
public class ContaController : AlunoBaseController
{
    private readonly IPasswordHasher<Models.Aluno> _hasher;

    public ContaController(ApplicationDbContext db, IPasswordHasher<Models.Aluno> hasher) : base(db)
    {
        _hasher = hasher;
    }

    // GET: /Aluno/Conta/AlterarSenha
    [HttpGet]
    public IActionResult AlterarSenha() => View(new AlterarSenhaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var aluno = await ObterAlunoAsync();
        if (aluno == null) return Challenge("Aluno");

        // Confere a senha atual.
        if (string.IsNullOrEmpty(aluno.SenhaHash) ||
            _hasher.VerifyHashedPassword(aluno, aluno.SenhaHash, model.SenhaAtual) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "A senha atual está incorreta.");
            return View(model);
        }

        aluno.SenhaHash = _hasher.HashPassword(aluno, model.NovaSenha);
        await Db.SaveChangesAsync();

        // Renova o cookie do aluno (mantém logado com os dados atualizados).
        await AlunoAuth.EntrarAsync(HttpContext, aluno, lembrar: true);

        TempData["Sucesso"] = "Senha alterada com sucesso!";
        return RedirectToAction(nameof(AlterarSenha));
    }
}
