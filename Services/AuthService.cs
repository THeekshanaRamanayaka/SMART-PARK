using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Data;
using SmartPark.Models;

namespace SmartPark.Services;

public class AuthService
{
    private readonly SmartParkDbContext _db;
    private readonly ILogger<AuthService> _logger;
    private readonly string _jwtKey;

    public AuthService(SmartParkDbContext db, ILogger<AuthService> logger)
    {
        _db = db;
        _logger = logger;
        _jwtKey = Environment.GetEnvironmentVariable("SMARTPARK_JWT_KEY") ?? "smartpark-dev-secret-key-minimum-32bytes!";
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string hash, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<(AppUser? user, string? accessToken, string? refreshToken)> AuthenticateAsync(string usernameOrEmail, string password)
    {
        var user = _db.AppUsers.FirstOrDefault(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        if (user == null || !VerifyPassword(user.PasswordHash, password) || !user.IsActive)
        {
            return (null, null, null);
        }

        var accessToken = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id).ConfigureAwait(false);

        return (user, accessToken, refreshToken);
    }

    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("fullName", user.FullName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: "SmartPark",
            audience: "SmartParkClient",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var refresh = new RefreshToken
        {
            AppUserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return refresh.Token;
    }

    public async Task<(AppUser? user, string? accessToken, string? refreshToken)> RefreshAsync(string refreshToken)
    {
        var stored = _db.RefreshTokens.FirstOrDefault(r => r.Token == refreshToken && !r.Revoked && r.ExpiresAt > DateTime.UtcNow);
        if (stored == null)
        {
            return (null, null, null);
        }

        var user = _db.AppUsers.FirstOrDefault(u => u.Id == stored.AppUserId);
        if (user == null)
        {
            return (null, null, null);
        }

        // Revoke old refresh token and issue a new one
        stored.Revoked = true;
        await _db.SaveChangesAsync().ConfigureAwait(false);

        var accessToken = GenerateJwtToken(user);
        var newRefresh = await CreateRefreshTokenAsync(user.Id).ConfigureAwait(false);

        return (user, accessToken, newRefresh);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var stored = _db.RefreshTokens.FirstOrDefault(r => r.Token == refreshToken && !r.Revoked);
        if (stored == null) return false;

        stored.Revoked = true;
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public (ClaimsPrincipal? principal, string? error) ValidateJwtToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return (null, "Token is missing");

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = "SmartPark",
            ValidateAudience = true,
            ValidAudience = "SmartParkClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            return (principal, null);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return (null, ex.Message);
        }
    }
}
