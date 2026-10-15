using CastJournal.Application.DTOs.Auth;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Services;
using Google.Apis.Auth;                            
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly GoogleAuthSettings _googleAuthSettings;
    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();


    public AuthController(UserManager<User> userManager, ITokenService tokenService, IAuditService auditService,
        IEmailService emailService, IOptions<EmailSettings> emailSettings, IOptions<GoogleAuthSettings> googleAuthSettings)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _emailService = emailService;
        _googleAuthSettings = googleAuthSettings.Value;
        _emailSettings = emailSettings.Value;
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Credential))
            return BadRequest("Missing Google credential.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleAuthSettings.ClientId }
            });
        }
        catch (InvalidJwtException)
        {
            await _auditService.LogAsync(null, null, AuditAction.LoginFailed,
                details: "Invalid Google credential", ipAddress: GetClientIp());
            return Unauthorized("Invalid Google sign-in. Please try again.");
        }

        if (!payload.EmailVerified)
            return Unauthorized("Your Google email address is not verified.");

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            // Brand-new account — Google has already verified the email,
            // so there's no separate confirmation step needed.
            user = new User
            {
                UserName = payload.Email,
                Email = payload.Email,
                FullName = payload.Name,
                AvatarUrl = payload.Picture,
                EmailConfirmed = true,
                GoogleId = payload.Subject,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, "User");
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.Register,
                details: "Registered via Google", ipAddress: GetClientIp());
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            // An existing email/password account signing in with Google for
            // the first time, using the same email — link the accounts
            // instead of creating a duplicate user.
            user.GoogleId = payload.Subject;
            await _userManager.UpdateAsync(user);
        }

        if (!user.IsActive)
        {
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.LoginFailed,
                details: "Account deactivated", ipAddress: GetClientIp());
            return Unauthorized("This account has been deactivated. Please contact an administrator.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _auditService.LogAsync(user.Id, user.Email, AuditAction.LoginFailed,
                details: "Account locked out", ipAddress: GetClientIp());
            return Unauthorized("This account is temporarily locked. Please try again later.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync(user.Id, user.Email, AuditAction.Login,
            details: "Signed in with Google", ipAddress: GetClientIp());

        var roles = await _userManager.GetRolesAsync(user);
        var tokenResponse = await _tokenService.GenerateTokenAsync(user, roles);
        return Ok(tokenResponse);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Always the same response whether or not the email exists —
        // otherwise this endpoint becomes a way to check which emails are registered.
        if (user != null && user.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_emailSettings.FrontendBaseUrl}/reset-password" +
                $"?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName ?? user.UserName ?? "Angler", resetLink);
        }

        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest("Invalid or expired reset link.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Your password has been reset. You can now log in." });
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