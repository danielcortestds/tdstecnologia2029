using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

/// <summary>
/// Diagnóstico temporário de contas (Master-only). Mostra o estado real de uma
/// conta Identity para investigar problemas de login. Não expõe senha nem hash.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Master")]
public class DiagController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DiagController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Conta(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Json(new { erro = "informe ?email=" });

        email = email.Trim();

        // Login usa FindByNameAsync (NormalizedUserName). Se achar por e-mail mas
        // NÃO por nome, é problema de normalização do UserName.
        var porNome = await _userManager.FindByNameAsync(email);
        var porEmail = await _userManager.FindByEmailAsync(email);
        var u = porNome ?? porEmail;

        if (u == null)
        {
            // Sem conta de login. Vê se existe PERFIL de aluno com esse e-mail,
            // para saber o caminho: editar um aluno existente × cadastrar do zero.
            var perfil = await _db.Alunos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == email);
            return Json(new
            {
                encontrado = false,
                achaPorNome = false,
                achaPorEmail = false,
                temPerfilAluno = perfil != null,
                alunoId = perfil?.Id,
                nomeAluno = perfil?.NomeCompleto,
                dica = perfil != null
                    ? "Existe o PERFIL de aluno, mas NÃO a conta de login. Em Admin → Alunos → Editar, defina uma senha e salve: o sistema cria a conta agora."
                    : "Não existe conta de login NEM perfil de aluno com esse e-mail. Cadastre em Admin → Alunos → Novo aluno (com senha)."
            });
        }

        var roles = await _userManager.GetRolesAsync(u);
        var aluno = await _db.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == u.Id);

        return Json(new
        {
            encontrado = true,
            achaPorNome = porNome != null,     // << crítico p/ o login
            achaPorEmail = porEmail != null,
            userName = u.UserName,
            normalizedUserName = u.NormalizedUserName,
            email = u.Email,
            normalizedEmail = u.NormalizedEmail,
            emailConfirmed = u.EmailConfirmed,
            temSenha = !string.IsNullOrEmpty(u.PasswordHash),
            temSecurityStamp = !string.IsNullOrEmpty(u.SecurityStamp),
            lockoutEnd = u.LockoutEnd,
            lockoutEnabled = u.LockoutEnabled,
            accessFailedCount = u.AccessFailedCount,
            papeis = string.Join(",", roles),
            temPerfilAluno = aluno != null,
            userIdDoPerfilBate = aluno == null ? (bool?)null : aluno.UserId == u.Id
        });
    }
}
