using AnimeWiki.Api.Data;
using AnimeWiki.Api.DTOs.Auth;
using AnimeWiki.Api.Models;
using AnimeWiki.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AnimeWiki.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _tokenService;

    public AuthController(AppDbContext db, IPasswordHasher<User> passwordHasher, JwtTokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>Реєструє нового користувача та повертає JWT-токен.</summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var normalizedUsername = username.ToUpperInvariant();
        var normalizedEmail = email.ToUpperInvariant();

        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail))
            return Conflict(new { message = "Ця електронна пошта вже зареєстрована." });

        if (await _db.Users.AnyAsync(x => x.NormalizedUsername == normalizedUsername))
            return Conflict(new { message = "Таке ім'я користувача вже зайняте." });

        var user = new User
        {
            Username = username,
            NormalizedUsername = normalizedUsername,
            Email = email,
            NormalizedEmail = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "Ця пошта або ім'я користувача вже використовується." });
        }

        return CreatedAtAction(nameof(Me), null, _tokenService.CreateToken(user));
    }

    /// <summary>Авторизує за поштою або ім'ям користувача та повертає JWT-токен.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var value = request.EmailOrUsername.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x =>
            x.NormalizedEmail == value || x.NormalizedUsername == value);

        if (user == null)
            return Unauthorized(new { message = "Неправильний логін або пароль." });

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Неправильний логін або пароль." });

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync();
        }

        return Ok(_tokenService.CreateToken(user));
    }

    /// <summary>Повертає профіль користувача за дійсним Bearer-токеном.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var subject = User.FindFirst("sub")?.Value;
        if (!int.TryParse(subject, out var id))
            return Unauthorized(new { message = "Некоректний токен." });

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return Unauthorized(new { message = "Користувача не знайдено." });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        });
    }
}
