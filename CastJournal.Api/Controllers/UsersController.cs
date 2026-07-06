using CastJournal.Application.DTOs.Users;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IAuditService _auditService;

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();


    public UsersController(IUserManagementService userManagementService, IAuditService auditService)
    {
        _userManagementService = userManagementService;
        _auditService = auditService;
    }

    // GET api/users?searchTerm=&role=&isActive=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
    {
        var result = await _userManagementService.GetUsersAsync(filter);
        return Ok(result);
    }

    // GET api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // POST api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var (result, user) = await _userManagementService.CreateUserAsync(dto);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(adminId, adminEmail, AuditAction.UserCreated,
            "User", user!.Id, $"Created user {user.Email} with role {user.Role}", GetClientIp());

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT api/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userManagementService.UpdateUserAsync(id, dto);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(adminId, adminEmail, AuditAction.UserUpdated, "User", id, ipAddress: GetClientIp());

        return NoContent();
    }

    // PATCH api/users/{id}/deactivate
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var result = await _userManagementService.SetActiveStatusAsync(id, false);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(adminId, adminEmail, AuditAction.UserDeactivated, "User", id, ipAddress: GetClientIp());

        return NoContent();
    }

    // PATCH api/users/{id}/reactivate
    [HttpPatch("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(string id)
    {
        var result = await _userManagementService.SetActiveStatusAsync(id, true);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(adminId, adminEmail, AuditAction.UserReactivated, "User", id, ipAddress: GetClientIp());

        return NoContent();
    }
}