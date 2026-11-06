using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Autenticação própria dos alunos (cookie "Aluno"), separada do ASP.NET Identity
/// usado pela administração. O aluno é identificado pelo claim "AlunoId".
/// </summary>
public static class AlunoAuth
{
    public const string Scheme = "Aluno";

    /// <summary>Faz o login do aluno no cookie "Aluno".</summary>
    public static async Task EntrarAsync(HttpContext http, Aluno aluno, bool lembrar)
    {
        var claims = new List<Claim>
        {
            new("AlunoId", aluno.Id.ToString()),
            new(ClaimTypes.Name, aluno.NomeCompleto),
            new(ClaimTypes.Email, aluno.Email),
        };
        var identity = new ClaimsIdentity(claims, Scheme);
        var props = new AuthenticationProperties
        {
            IsPersistent = lembrar,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(lembrar ? 14 : 1),
        };
        await http.SignInAsync(Scheme, new ClaimsPrincipal(identity), props);
    }

    public static Task SairAsync(HttpContext http) => http.SignOutAsync(Scheme);
}
