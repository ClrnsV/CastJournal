using CastJournal.Application.DTOs.Analytics;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CastJournal.API.Controllers;

[Route("api/analytics")]
[ApiController]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<ActionResult<AnalyticsDto>> GetAnalytics([FromQuery] AnalyticsFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var analytics = await _analyticsService.GetAnalyticsAsync(userId, filter);
        return Ok(analytics);
    }
}