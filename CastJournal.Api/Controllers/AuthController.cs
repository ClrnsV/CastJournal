using CastJournal.Application.DTOs.Auth;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();


    public AuthController(UserManager<User> userManager, ITokenService tokenService, IAuditService auditService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _auditService = auditService;
    }

    // POST: api/auth/register
    // Need to handle the email uniqueness in registration process to return a proper error message if the email is already in use and cant be registered again before publishing again.
    //Check daan sa userManager.CreateAsync if naay logic nga the email is already in use and return a proper error message if it is.
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var user = new User { UserName = dto.UserName, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "User");
        await _auditService.LogAsync(user.Id, user.Email, AuditAction.Register, ipAddress: GetClientIp());

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResponse = await _tokenService.GenerateTokenAsync(user, roles);
        return Ok(tokenResponse);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            await _auditService.LogAsync(null, dto.Email, AuditAction.LoginFailed,
                details: "Invalid email or password", ipAddress: GetClientIp());
            return Unauthorized("Invalid email or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.LoginFailed,
                details: "Account locked out due to repeated failed login attempts", ipAddress: GetClientIp());
            return Unauthorized("This account is temporarily locked due to repeated failed login attempts. Please try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.LoginFailed,
                details: "Invalid email or password", ipAddress: GetClientIp());
            return Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.LoginFailed,
                details: "Account deactivated", ipAddress: GetClientIp());
            return Unauthorized("This account has been deactivated. Please contact an administrator.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync(user.Id, user.Email, AuditAction.Login, ipAddress: GetClientIp());

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResponse = await _tokenService.GenerateTokenAsync(user, roles);
        return Ok(tokenResponse);
    }

    //// POST: api/auth/logout
    //[HttpPost("logout")]
    //[Authorize]
    //public async Task<IActionResult> Logout()
    //{
    //    var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var email = User.FindFirstValue(ClaimTypes.Email);
    //    var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

    //    if (jti == null || userId == null || expClaim == null)
    //        return BadRequest("Invalid token.");

    //    var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
    //    await _tokenService.RevokeTokenAsync(jti, userId, expiresAt);
    //    await _auditService.LogAsync(userId, email, AuditAction.Logout, ipAddress: GetClientIp());

    //    return Ok(new { message = "Logged out successfully." });
    //}

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto dto)
    {
        var result = await _tokenService.RefreshTokenAsync(dto.RefreshToken);
        if (result == null)
            return Unauthorized("Invalid or expired refresh token. Please log in again.");

        return Ok(result);
    }

    // POST: api/auth/logout — now accepts the refresh token so it can be revoked too
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (jti == null || userId == null || expClaim == null)
            return BadRequest("Invalid token.");

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
        await _tokenService.RevokeTokenAsync(jti, userId, expiresAt);

        if (!string.IsNullOrEmpty(dto?.RefreshToken))
            await _tokenService.RevokeRefreshTokenAsync(dto.RefreshToken);

        await _auditService.LogAsync(userId, email, AuditAction.Logout, ipAddress: GetClientIp());

        return Ok(new { message = "Logged out successfully." });
    }
}