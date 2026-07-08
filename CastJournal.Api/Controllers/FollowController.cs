using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/users/{id}/follow")]
[ApiController]
[Authorize]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpPost]
    public async Task<IActionResult> Follow(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var (succeeded, error) = await _followService.FollowAsync(currentUserId, id);
        return succeeded ? NoContent() : BadRequest(error);
    }

    [HttpDelete]
    public async Task<IActionResult> Unfollow(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var (succeeded, error) = await _followService.UnfollowAsync(currentUserId, id);
        return succeeded ? NoContent() : BadRequest(error);
    }
}