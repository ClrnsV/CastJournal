using CastJournal.Application.DTOs.Search;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/search")]
[ApiController]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    // GET api/search?query=...&type=all|users|catches&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] string type = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new SearchResultDto());

        var currentUserId = GetCurrentUserId();
        var result = await _searchService.SearchAsync(query.Trim(), type, currentUserId, page, pageSize);
        return Ok(result);
    }
}