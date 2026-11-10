using System.ComponentModel.DataAnnotations;

namespace AnimeWiki.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Вкажіть ім'я користувача.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача має містити від 3 до 50 символів.")]
    [RegularExpression(@"^[\p{L}\p{N}_.-]+$", ErrorMessage = "У імені дозволені літери, цифри, _, . та -.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Некоректна електронна пошта.")]
    [StringLength(256, ErrorMessage = "Електронна пошта занадто довга.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Пароль має містити від 8 до 128 символів.")]
    public string Password { get; set; } = string.Empty;
}
