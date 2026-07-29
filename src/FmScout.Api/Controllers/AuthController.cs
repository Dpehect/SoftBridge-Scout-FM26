using FmScout.Api.Auth;
using FmScout.Domain.Entities;
using FmScout.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FmScout.Api.Controllers;
[ApiController, Route("api/auth")]
public sealed class AuthController(FmScoutDbContext db, JwtTokenService tokens, IPasswordHasher<UserAccount> hasher) : ControllerBase
{
    public sealed record RegisterRequest(string Email, string DisplayName, string Password);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Password.Length < 8) return BadRequest(new { message = "Password must contain at least 8 characters." });
        if (await db.UserAccounts.AnyAsync(x => x.Email == email, ct)) return Conflict(new { message = "Email already exists." });
        var user = new UserAccount { Email = email, DisplayName = request.DisplayName.Trim() };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.UserAccounts.Add(user); await db.SaveChangesAsync(ct);
        return Ok(await IssueAsync(user, ct));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.UserAccounts.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower() && x.IsActive, ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized(new { message = "Invalid credentials." });
        return Ok(await IssueAsync(user, ct));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var hash = JwtTokenService.Hash(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(x => x.UserAccount).SingleOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow, ct);
        if (stored is null) return Unauthorized(); stored.RevokedAt = DateTimeOffset.UtcNow;
        return Ok(await IssueAsync(stored.UserAccount, ct));
    }

    private async Task<object> IssueAsync(UserAccount user, CancellationToken ct)
    {
        var refresh = JwtTokenService.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken { UserAccountId = user.Id, TokenHash = JwtTokenService.Hash(refresh), ExpiresAt = DateTimeOffset.UtcNow.AddDays(30) });
        await db.SaveChangesAsync(ct);
        return new { accessToken = tokens.CreateAccessToken(user), refreshToken = refresh, user = new { user.Id, user.Email, user.DisplayName, user.Role } };
    }
}
