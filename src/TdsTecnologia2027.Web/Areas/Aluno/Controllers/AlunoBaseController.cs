using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Areas.Aluno.Controllers;

[Area("Aluno")]
[Authorize(AuthenticationSchemes = "Aluno")]
public abstract class AlunoBaseController : Controller
{
    protected readonly ApplicationDbContext Db;

    protected AlunoBaseController(ApplicationDbContext db) => Db = db;

    /// <summary>Aluno logado, identificado pelo claim "AlunoId" do cookie "Aluno".</summary>
    protected async Task<Models.Aluno?> ObterAlunoAsync()
    {
        var idTexto = User.FindFirstValue("AlunoId");
        if (!int.TryParse(idTexto, out var id)) return null;
        return await Db.Alunos.FirstOrDefaultAsync(a => a.Id == id);
    }
}
