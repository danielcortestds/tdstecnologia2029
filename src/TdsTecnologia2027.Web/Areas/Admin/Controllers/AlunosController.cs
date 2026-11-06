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
    private readonly IPasswordHasher<Models.Aluno> _hasher;

    public AlunosController(ApplicationDbContext db, IPasswordHasher<Models.Aluno> hasher)
    {
        _db = db;
        _hasher = hasher;
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
        var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await _db.Alunos.AnyAsync(a => a.Email.ToLower() == email))
            ModelState.AddModelError(nameof(model.Email), "Já existe um aluno com este e-mail.");

        if (!ModelState.IsValid) return View(model);

        var aluno = new Models.Aluno
        {
            NomeCompleto = model.NomeCompleto,
            Email = email,
            Cpf = model.Cpf,
            Telefone = model.Telefone,
            Empresa = model.Empresa,
            Status = model.Status
        };
        aluno.SenhaHash = _hasher.HashPassword(aluno, model.Senha);
        _db.Alunos.Add(aluno);
        await _db.SaveChangesAsync();

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
            SemSenha = string.IsNullOrEmpty(aluno.SenhaHash),
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
            model.SemSenha = string.IsNullOrEmpty(aluno.SenhaHash);
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

        // Senha: define/reseta se informada. Se o aluno ainda não tem senha, exige uma.
        if (!string.IsNullOrWhiteSpace(model.NovaSenha))
        {
            aluno.SenhaHash = _hasher.HashPassword(aluno, model.NovaSenha);
        }
        else if (string.IsNullOrEmpty(aluno.SenhaHash))
        {
            ModelState.AddModelError(nameof(model.NovaSenha),
                "Este aluno ainda não tem senha de acesso. Defina uma senha para ele poder entrar.");
            model.Email = aluno.Email;
            model.SemSenha = true;
            return View(model);
        }

        await _db.SaveChangesAsync();

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
            _db.Alunos.Remove(aluno); // matrículas em cascata
            await _db.SaveChangesAsync();
            TempData["Sucesso"] = "Aluno excluído.";
        }
        return RedirectToAction(nameof(Index));
    }
}
