using System.ComponentModel.DataAnnotations;

namespace TdsTecnologia2027.Web.Models;

/// <summary>Linha da listagem de usuários do admin.</summary>
public class UsuarioListaItem
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
}

public class NovoUsuarioViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escolha o nível de acesso.")]
    [Display(Name = "Nível de acesso")]
    public string Papel { get; set; } = "Editor";
}

public class EditarUsuarioViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escolha o nível de acesso.")]
    [Display(Name = "Nível de acesso")]
    public string Papel { get; set; } = "Editor";

    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha (opcional)")]
    public string? NovaSenha { get; set; }
}
