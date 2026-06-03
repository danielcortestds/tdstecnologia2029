using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class UsuariosController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private static readonly string[] PapeisValidos = { "Master", "Editor" };

    public UsuariosController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: Admin/Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var lista = new List<UsuarioListaItem>();
        foreach (var u in usuarios)
        {
            var papeis = await _userManager.GetRolesAsync(u);
            lista.Add(new UsuarioListaItem
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "(sem e-mail)",
                Papel = papeis.FirstOrDefault() ?? "(sem papel)"
            });
        }
        return View(lista);
    }

    // GET: Admin/Usuarios/Create
    public IActionResult Create()
    {
        CarregarPapeis();
        return View(new NovoUsuarioViewModel());
    }

    // POST: Admin/Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovoUsuarioViewModel model)
    {
        if (!PapeisValidos.Contains(model.Papel))
            ModelState.AddModelError(nameof(model.Papel), "Nível de acesso inválido.");

        if (await _userManager.FindByEmailAsync(model.Email) != null)
            ModelState.AddModelError(nameof(model.Email), "Já existe um usuário com este e-mail.");

        if (!ModelState.IsValid)
        {
            CarregarPapeis();
            return View(model);
        }

        var usuario = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(usuario, model.Senha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, TraduzErro(erro));
            CarregarPapeis();
            return View(model);
        }

        await _userManager.AddToRoleAsync(usuario, model.Papel);
        TempData["Sucesso"] = $"Usuário {model.Email} criado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Usuarios/Edit/{id}
    public async Task<IActionResult> Edit(string? id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        var papeis = await _userManager.GetRolesAsync(usuario);
        CarregarPapeis();
        return View(new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            Email = usuario.Email ?? string.Empty,
            Papel = papeis.FirstOrDefault() ?? "Editor"
        });
    }

    // POST: Admin/Usuarios/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarUsuarioViewModel model)
    {
        if (!PapeisValidos.Contains(model.Papel))
            ModelState.AddModelError(nameof(model.Papel), "Nível de acesso inválido.");

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Email = usuario.Email ?? string.Empty;
            CarregarPapeis();
            return View(model);
        }

        // Impede rebaixar o último Master (evita travar o acesso total ao sistema).
        var papeisAtuais = await _userManager.GetRolesAsync(usuario);
        if (papeisAtuais.Contains("Master") && model.Papel != "Master" && await ContarMasters() <= 1)
        {
            ModelState.AddModelError(string.Empty, "Não é possível rebaixar o único usuário Master do sistema.");
            model.Email = usuario.Email ?? string.Empty;
            CarregarPapeis();
            return View(model);
        }

        // Atualiza o papel
        await _userManager.RemoveFromRolesAsync(usuario, papeisAtuais);
        await _userManager.AddToRoleAsync(usuario, model.Papel);

        // Troca a senha, se informada
        if (!string.IsNullOrWhiteSpace(model.NovaSenha))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var res = await _userManager.ResetPasswordAsync(usuario, token, model.NovaSenha);
            if (!res.Succeeded)
            {
                foreach (var erro in res.Errors)
                    ModelState.AddModelError(string.Empty, TraduzErro(erro));
                model.Email = usuario.Email ?? string.Empty;
                CarregarPapeis();
                return View(model);
            }
        }

        TempData["Sucesso"] = $"Usuário {usuario.Email} atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Usuarios/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        // Não permite excluir a si mesmo
        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Erro"] = "Você não pode excluir o seu próprio usuário.";
            return RedirectToAction(nameof(Index));
        }

        // Não permite excluir o último Master
        if (await _userManager.IsInRoleAsync(usuario, "Master") && await ContarMasters() <= 1)
        {
            TempData["Erro"] = "Não é possível excluir o único usuário Master do sistema.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(usuario);
        TempData["Sucesso"] = $"Usuário {usuario.Email} excluído.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<int> ContarMasters()
    {
        var masters = await _userManager.GetUsersInRoleAsync("Master");
        return masters.Count;
    }

    private void CarregarPapeis()
    {
        ViewBag.Papeis = new List<SelectListItem>
        {
            new() { Value = "Master", Text = "Master — acesso total" },
            new() { Value = "Editor", Text = "Editor — somente cursos" }
        };
    }

    private static string TraduzErro(IdentityError erro) => erro.Code switch
    {
        "PasswordTooShort" => "A senha é muito curta.",
        "PasswordRequiresNonAlphanumeric" => "A senha precisa de um caractere especial.",
        "PasswordRequiresDigit" => "A senha precisa de um número.",
        "PasswordRequiresUpper" => "A senha precisa de uma letra maiúscula.",
        "PasswordRequiresLower" => "A senha precisa de uma letra minúscula.",
        "DuplicateUserName" or "DuplicateEmail" => "Já existe um usuário com este e-mail.",
        _ => erro.Description
    };
}
