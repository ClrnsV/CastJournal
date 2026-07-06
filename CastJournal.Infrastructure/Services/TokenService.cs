using CastJournal.Application.DTOs.Auth;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CastJournal.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRevokedTokenRepository _revokedTokenRepository;


    public TokenService(IOptions<JwtSettings> jwtSettings, IRevokedTokenRepository revokedTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _revokedTokenRepository = revokedTokenRepository;
    }

    public Task<AuthResponseDto> GenerateTokenAsync(User user, IList<string> roles)
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

        return Task.FromResult(new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName,
            Expiration = expiration
        });
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
}