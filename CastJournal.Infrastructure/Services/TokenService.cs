using CastJournal.Application.DTOs.Auth;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace CastJournal.Infrastructure.Services;
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRevokedTokenRepository _revokedTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<User> _userManager;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRevokedTokenRepository revokedTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        _revokedTokenRepository = revokedTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
    }

    public async Task<AuthResponseDto> GenerateTokenAsync(User user, IList<string> roles)
    {
        var jti = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("sub", user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        var refreshTokenPlain = GenerateSecureToken();
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = HashToken(refreshTokenPlain),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
        });

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshTokenPlain,
            UserId = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName,
            Expiration = expiration
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var hashed = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenAsync(hashed);

        if (stored == null || !stored.IsActive)
            return null;

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user == null || !user.IsActive)
            return null;

        // Rotation: this refresh token is now spent, whether or not the
        // caller ever uses the new pair. Prevents replay of an old one.
        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(stored);

        var roles = await _userManager.GetRolesAsync(user);
        return await GenerateTokenAsync(user, roles);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        var hashed = HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenAsync(hashed);
        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);
        }
    }

    public async Task RevokeTokenAsync(string jti, string userId, DateTime expiresAt)
    {
        await _revokedTokenRepository.AddAsync(new RevokedToken
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt
        });
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}