using CastJournal.Application.DTOs.Species;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/species")]
[ApiController]
[Authorize]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesService _speciesService;
    private readonly IAuditService _auditService;

    public SpeciesController(ISpeciesService speciesService, IAuditService auditService)
    {
        _speciesService = speciesService;
        _auditService = auditService;
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    // Read access stays open to any logged-in user — everyone needs to browse species when logging a catch.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpeciesDto>>> GetAll()
    {
        var species = await _speciesService.GetAllSpeciesAsync();
        return Ok(species);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpeciesDto>> GetById(Guid id)
    {
        var species = await _speciesService.GetSpeciesByIdAsync(id);
        return species != null ? Ok(species) : NotFound();
    }

    // Mutations are Admin-only per UC-10 — this was previously open to any authenticated user.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpeciesDto>> Create([FromBody] CreateSpeciesDto dto)
    {
        var species = await _speciesService.CreateSpeciesAsync(dto);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(userId, email, AuditAction.SpeciesCreated,
            "Species", species.Id.ToString(), $"Created species '{species.CommonName}'", GetClientIp());

        return CreatedAtAction(nameof(GetById), new { id = species.Id }, species);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpeciesDto>> Update(Guid id, [FromBody] CreateSpeciesDto dto)
    {
        var species = await _speciesService.UpdateSpeciesAsync(id, dto);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(userId, email, AuditAction.SpeciesUpdated,
            "Species", id.ToString(), $"Updated species '{species.CommonName}'", GetClientIp());

        return Ok(species);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _speciesService.DeleteSpeciesAsync(id);
        if (!success)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _auditService.LogAsync(userId, email, AuditAction.SpeciesDeleted,
            "Species", id.ToString(), ipAddress: GetClientIp());

        return NoContent();
    }
}