using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace TdsTecnologia2027.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetPasswordModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Concluido { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha.")]
        [StringLength(100, ErrorMessage = "A senha precisa ter pelo menos {2} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova senha")]
        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? code = null, string? email = null)
    {
        if (string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, "Link de redefinição inválido. Peça um novo em \"Esqueci minha senha\".");
            return Page();
        }
        Input = new InputModel { Code = code, Email = email ?? string.Empty };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Não revela que o e-mail não existe.
            Concluido = true;
            return Page();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Link de redefinição inválido ou expirado. Peça um novo.");
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
        if (result.Succeeded)
        {
            Concluido = true;
            return Page();
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, Traduzir(e.Code, e.Description));
        return Page();
    }

    private static string Traduzir(string code, string descricao) => code switch
    {
        "InvalidToken" => "Link de redefinição inválido ou expirado. Peça um novo em \"Esqueci minha senha\".",
        "PasswordTooShort" => "A senha é muito curta (mínimo 8 caracteres).",
        "PasswordRequiresNonAlphanumeric" => "A senha precisa de um caractere especial (ex.: @, !, #).",
        "PasswordRequiresDigit" => "A senha precisa de um número.",
        "PasswordRequiresUpper" => "A senha precisa de uma letra maiúscula.",
        "PasswordRequiresLower" => "A senha precisa de uma letra minúscula.",
        _ => descricao
    };
}
