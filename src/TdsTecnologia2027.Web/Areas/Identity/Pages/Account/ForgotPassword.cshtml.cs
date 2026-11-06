using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TdsTecnologia2027.Web.Services;

namespace TdsTecnologia2027.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _email;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender email, ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _email = email;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Enviado { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Mostra a mesma confirmação sempre, sem revelar se o e-mail existe.
        Enviado = true;

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user != null)
        {
            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = Input.Email },
                    protocol: Request.Scheme)!;

                var html = $@"<p>Olá,</p>
<p>Recebemos um pedido para redefinir a senha da sua conta na <strong>TDS Tecnologia</strong>.</p>
<p>Clique no botão abaixo para criar uma nova senha:</p>
<p><a href=""{callbackUrl}"" style=""background:#0d9488;color:#ffffff;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:600;display:inline-block;"">Criar nova senha</a></p>
<p>Se o botão não funcionar, copie e cole este endereço no navegador:<br><a href=""{callbackUrl}"">{callbackUrl}</a></p>
<p style=""color:#64748b;font-size:13px;"">Se você não pediu isso, pode ignorar este e-mail — sua senha continua a mesma.</p>";

                await _email.EnviarAsync(Input.Email, "Redefinição de senha — TDS Tecnologia", html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail de redefinição para {Email}.", Input.Email);
            }
        }

        return Page();
    }
}
