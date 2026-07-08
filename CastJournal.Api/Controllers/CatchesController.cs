using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CatchesController : ControllerBase
{
    private readonly ICatchService _catchService;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    public CatchesController(ICatchService catchService, IMapper mapper, IAuditService auditService)
    {
        _catchService = catchService;
        _mapper = mapper;
        _auditService = auditService;
    }

    // Helper to get current user ID from JWT
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
    }

    [HttpPost]
    public async Task<ActionResult<CatchDto>> CreateCatch([FromBody] CreateCatchDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");
        var result = await _catchService.CreateCatchAsync(dto, userId);
        return CreatedAtAction(nameof(GetCatch), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatchDto>>> GetCatches()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");
        var catches = await _catchService.GetUserCatchesAsync(userId);
        return Ok(catches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CatchDto>> GetCatch(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");
        var catchRecord = await _catchService.GetCatchByIdAsync(id, userId);
        if (catchRecord == null) return NotFound();
        return Ok(catchRecord);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCatch(Guid id, [FromBody] CreateCatchDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var success = await _catchService.UpdateCatchAsync(id, dto, userId);
        if (!success)
            return NotFound("Catch not found or unauthorized");
        return NoContent(); // 204 No Content
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCatch(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var success = await _catchService.DeleteCatchAsync(id, userId);
        if (!success)
            return NotFound("Catch not found or unauthorized");

        // Logged only after the delete is confirmed to have actually happened.
        await _auditService.LogAsync(userId, User.FindFirstValue(ClaimTypes.Email), AuditAction.CatchDeleted,
            "Catch", id.ToString(), ipAddress: GetClientIp());

        return NoContent(); // 204 No Content
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<CatchDto>>> SearchCatches([FromQuery] CatchFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");
        var result = await _catchService.SearchCatchesAsync(userId, filter);
        return Ok(result);
    }
    [HttpGet("feed")]
    public async Task<ActionResult<PagedResult<CatchDto>>> GetPublicFeed([FromQuery] CatchFeedFilterDto filter)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _catchService.GetPublicFeedAsync(filter, currentUserId);
        return Ok(result);
    }
}