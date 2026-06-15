using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Master")]
public class AlunosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AlunosController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin/Alunos
    public async Task<IActionResult> Index()
    {
        var alunos = await _db.Alunos
            .Include(a => a.Matriculas)
            .OrderBy(a => a.NomeCompleto)
            .Select(a => new AlunoListaItem
            {
                Id = a.Id,
                NomeCompleto = a.NomeCompleto,
                Email = a.Email,
                Empresa = a.Empresa,
                Status = a.Status,
                CursosLiberados = a.Matriculas.Count(m => m.Ativo)
            })
            .ToListAsync();
        return View(alunos);
    }

    // GET: Admin/Alunos/Create
    public IActionResult Create() => View(new NovoAlunoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovoAlunoViewModel model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) != null)
            ModelState.AddModelError(nameof(model.Email), "Já existe um usuário com este e-mail.");

        if (!ModelState.IsValid) return View(model);

        await GarantirPapelAlunoAsync();

        var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
        var resultado = await _userManager.CreateAsync(user, model.Senha);
        if (!resultado.Succeeded)
        {
            foreach (var e in resultado.Errors) ModelState.AddModelError(string.Empty, TraduzErro(e));
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Aluno");

        var aluno = new Models.Aluno
        {
            UserId = user.Id,
            NomeCompleto = model.NomeCompleto,
            Email = model.Email,
            Cpf = model.Cpf,
            Telefone = model.Telefone,
            Empresa = model.Empresa,
            Status = model.Status
        };
        _db.Alunos.Add(aluno);
        await _db.SaveChangesAsync();

        await AplicarStatusAsync(user, model.Status);

        TempData["Sucesso"] = $"Aluno {model.NomeCompleto} criado! Agora libere os cursos.";
        return RedirectToAction(nameof(Edit), new { id = aluno.Id });
    }

    // GET: Admin/Alunos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var aluno = await _db.Alunos.Include(a => a.Matriculas).FirstOrDefaultAsync(a => a.Id == id);
        if (aluno == null) return NotFound();

        var cursos = await _db.CursosOnline.OrderBy(c => c.Nome).ToListAsync();
        var vm = new EditarAlunoViewModel
        {
            Id = aluno.Id,
            NomeCompleto = aluno.NomeCompleto,
            Email = aluno.Email,
            Cpf = aluno.Cpf,
            Telefone = aluno.Telefone,
            Empresa = aluno.Empresa,
            Status = aluno.Status,
            Acessos = cursos.Select(c =>
            {
                var m = aluno.Matriculas.FirstOrDefault(x => x.CursoOnlineId == c.Id);
                return new AcessoCursoItem
                {
                    CursoOnlineId = c.Id,
                    CursoNome = c.Nome,
                    Liberado = m?.Ativo ?? false,
                    DataInicio = m?.DataInicio,
                    DataFim = m?.DataFim
                };
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarAlunoViewModel model)
    {
        var aluno = await _db.Alunos.Include(a => a.Matriculas).FirstOrDefaultAsync(a => a.Id == model.Id);
        if (aluno == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Email = aluno.Email;
            return View(model);
        }

        aluno.NomeCompleto = model.NomeCompleto;
        aluno.Cpf = model.Cpf;
        aluno.Telefone = model.Telefone;
        aluno.Empresa = model.Empresa;
        aluno.Status = model.Status;

        // Acessos aos cursos
        foreach (var item in model.Acessos)
        {
            var matricula = aluno.Matriculas.FirstOrDefault(m => m.CursoOnlineId == item.CursoOnlineId);
            if (item.Liberado)
            {
                if (matricula == null)
                {
                    matricula = new MatriculaCurso { AlunoId = aluno.Id, CursoOnlineId = item.CursoOnlineId };
                    _db.Matriculas.Add(matricula);
                }
                matricula.Ativo = true;
                matricula.DataInicio = item.DataInicio;
                matricula.DataFim = item.DataFim;
            }
            else if (matricula != null)
            {
                matricula.Ativo = false; // remove o acesso (mantém histórico)
            }
        }

        await _db.SaveChangesAsync();

        // Identity: senha e status (bloqueio)
        var user = await _userManager.FindByIdAsync(aluno.UserId);
        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, model.NovaSenha);
                if (!res.Succeeded)
                {
                    foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, TraduzErro(e));
                    model.Email = aluno.Email;
                    return View(model);
                }
            }
            await AplicarStatusAsync(user, model.Status);
        }

        TempData["Sucesso"] = $"Aluno {aluno.NomeCompleto} atualizado!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno != null)
        {
            var user = await _userManager.FindByIdAsync(aluno.UserId);
            _db.Alunos.Remove(aluno); // matrículas em cascata
            await _db.SaveChangesAsync();
            if (user != null) await _userManager.DeleteAsync(user);
            TempData["Sucesso"] = "Aluno excluído.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private async Task GarantirPapelAlunoAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Aluno"))
            await _roleManager.CreateAsync(new IdentityRole("Aluno"));
    }

    /// <summary>Bloqueia (lockout) o login quando o aluno não está Ativo.</summary>
    private async Task AplicarStatusAsync(IdentityUser user, StatusAluno status)
    {
        await _userManager.SetLockoutEnabledAsync(user, true);
        if (status == StatusAluno.Ativo)
            await _userManager.SetLockoutEndDateAsync(user, null);
        else
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
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
