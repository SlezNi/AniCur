using System.ComponentModel.DataAnnotations;

namespace AnimeWiki.Api.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Вкажіть пошту або ім'я користувача.")]
    [StringLength(256)]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль.")]
    public string Password { get; set; } = string.Empty;
}
