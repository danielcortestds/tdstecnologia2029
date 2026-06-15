using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

[Area("Aluno")]
[Authorize(Roles = "Aluno")]
public abstract class AlunoBaseController : Controller
{
    protected readonly ApplicationDbContext Db;

    protected AlunoBaseController(ApplicationDbContext db) => Db = db;

    protected async Task<Models.Aluno?> ObterAlunoAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        return await Db.Alunos.FirstOrDefaultAsync(a => a.UserId == userId);
    }
}
