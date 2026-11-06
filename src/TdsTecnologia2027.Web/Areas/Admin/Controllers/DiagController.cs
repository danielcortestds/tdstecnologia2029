using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;

namespace TdsTecnologia2027.Web.Areas.Admin.Controllers;

/// <summary>
/// Diagnóstico temporário de conta de ALUNO (Master-only). Mostra o estado real
/// de um aluno na tabela própria (login separado do admin). Não expõe o hash.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Master")]
public class DiagController : Controller
{
    private readonly ApplicationDbContext _db;

    public DiagController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Conta(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Json(new { erro = "informe ?email=" });

        email = email.Trim().ToLowerInvariant();
        var aluno = await _db.Alunos.AsNoTracking()
            .Include(a => a.Matriculas)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email);

        if (aluno == null)
            return Json(new
            {
                encontrado = false,
                dica = "Nenhum ALUNO com esse e-mail. Cadastre em Admin → Alunos → Novo aluno (com senha)."
            });

        return Json(new
        {
            encontrado = true,
            alunoId = aluno.Id,
            nome = aluno.NomeCompleto,
            email = aluno.Email,
            temSenha = !string.IsNullOrEmpty(aluno.SenhaHash),
            status = aluno.Status.ToString(),
            podeLogar = !string.IsNullOrEmpty(aluno.SenhaHash) && aluno.Status == Models.StatusAluno.Ativo,
            cursosAtivos = aluno.Matriculas.Count(m => m.Ativo),
            dica = string.IsNullOrEmpty(aluno.SenhaHash)
                ? "O aluno existe mas está SEM SENHA. Em Admin → Alunos → Editar, defina uma senha e salve."
                : aluno.Status != Models.StatusAluno.Ativo
                    ? $"O aluno tem senha, mas o status é {aluno.Status} (login bloqueado). Mude para Ativo."
                    : "OK: aluno com senha e ativo. Deve conseguir entrar em /Aluno/Entrar."
        });
    }
}
