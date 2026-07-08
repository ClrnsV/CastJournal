using CastJournal.Application.DTOs.Locations;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/locations")]
[ApiController]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly IAuditService _auditService;

    public LocationsController(ILocationService locationService, IAuditService auditService)
    {
        _locationService = locationService;
        _auditService = auditService;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetUserLocations()
    {
        var userId = GetCurrentUserId();
        var locations = await _locationService.GetUserLocationsAsync(userId);
        return Ok(locations);
    }
    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetPublicLocations()
    {
        var locations = await _locationService.GetPublicLocationsAsync();
        return Ok(locations);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto dto)
    {
        var userId = GetCurrentUserId();
        var location = await _locationService.CreateLocationAsync(dto, userId);
        return CreatedAtAction(nameof(GetUserLocations), new { id = location.Id }, location);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LocationDto>> UpdateLocation(Guid id, [FromBody] CreateLocationDto dto)
    {
        var userId = GetCurrentUserId();
        var location = await _locationService.UpdateLocationAsync(id, dto, userId);
        return Ok(location);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        var userId = GetCurrentUserId();
        var success = await _locationService.DeleteLocationAsync(id, userId);

        if (!success)
            return NotFound("Location not found or unauthorized");

        // Logged only after the delete is confirmed to have actually happened.
        await _auditService.LogAsync(userId, User.FindFirstValue(ClaimTypes.Email), AuditAction.LocationDeleted,
            "FishingLocation", id.ToString(), ipAddress: GetClientIp());

        return NoContent();
    }
}