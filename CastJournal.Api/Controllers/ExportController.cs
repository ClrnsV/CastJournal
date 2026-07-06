using CastJournal.Application.DTOs.Export;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    // GET api/export/catches?format=Csv&startDate=2026-01-01&includeAnalytics=true
    [HttpGet("catches")]
    public async Task<IActionResult> ExportCatches([FromQuery] ExportRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var result = await _exportService.ExportCatchesAsync(userId, request);
        return File(result.FileContent, result.ContentType, result.FileName);
    }
}