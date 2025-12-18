using System.Security.Claims;
using CA2SOA.Auth;
using CA2SOA.Data;
using CA2SOA.Entities;
using CA2SOA.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CA2SOA.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly GameShelfDbContext _db;
    private readonly IUserService _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public AuthController(
        GameShelfDbContext db,
        IUserService users,
        IPasswordHasher hasher,
        IJwtTokenService tokens)
    {
        _db = db;
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var userName = (req.UserName ?? "").Trim();
        var email = (req.Email ?? "").Trim();
        var password = req.Password ?? "";

        if (userName.Length < 3) return BadRequest(new { message = "Username must be at least 3 characters." });
        if (password.Length < 6) return BadRequest(new { message = "Password must be at least 6 characters." });

        var exists = await _db.Users.AnyAsync(u =>
            u.UserName.ToLower() == userName.ToLower() ||
            (!string.IsNullOrWhiteSpace(email) && u.Email.ToLower() == email.ToLower())
        );

        if (exists) return Conflict(new { message = "Username or email already in use." });

        var (hash, salt) = _hasher.HashPassword(password);

        var user = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedUtc = DateTime.UtcNow
        };

        await _users.AddAsync(user);

        var token = _tokens.CreateToken(user);
        return Ok(new AuthResponse(token, new AuthUserDto(user.Id, user.UserName, user.Email)));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var key = (req.UserNameOrEmail ?? "").Trim();
        var password = req.Password ?? "";

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(password))
            return BadRequest(new { message = "Missing credentials." });

        User? user;
        if (key.Contains("@"))
            user = await _users.GetByEmailAsync(key);
        else
            user = await _users.GetByUserNameAsync(key);

        if (user is null) return Unauthorized(new { message = "Invalid credentials." });

        var ok = _hasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        if (!ok) return Unauthorized(new { message = "Invalid credentials." });

        var token = _tokens.CreateToken(user);
        return Ok(new AuthResponse(token, new AuthUserDto(user.Id, user.UserName, user.Email)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var id)) return Unauthorized();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return Unauthorized();

        return Ok(new AuthUserDto(user.Id, user.UserName, user.Email));
    }
}
