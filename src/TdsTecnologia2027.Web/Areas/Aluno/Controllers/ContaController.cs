using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

/// <summary>Conta do aluno: troca de senha.</summary>
public class ContaController : AlunoBaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public ContaController(ApplicationDbContext db, UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager) : base(db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Aluno/Conta/AlterarSenha
    [HttpGet]
    public IActionResult AlterarSenha() => View(new AlterarSenhaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var resultado = await _userManager.ChangePasswordAsync(user, model.SenhaAtual, model.NovaSenha);
        if (!resultado.Succeeded)
        {
            foreach (var e in resultado.Errors) ModelState.AddModelError(string.Empty, TraduzErro(e));
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Sucesso"] = "Senha alterada com sucesso!";
        return RedirectToAction(nameof(AlterarSenha));
    }

    private static string TraduzErro(IdentityError erro) => erro.Code switch
    {
        "PasswordMismatch" => "A senha atual está incorreta.",
        "PasswordTooShort" => "A nova senha é muito curta.",
        "PasswordRequiresNonAlphanumeric" => "A nova senha precisa de um caractere especial.",
        "PasswordRequiresDigit" => "A nova senha precisa de um número.",
        "PasswordRequiresUpper" => "A nova senha precisa de uma letra maiúscula.",
        "PasswordRequiresLower" => "A nova senha precisa de uma letra minúscula.",
        _ => erro.Description
    };
}
