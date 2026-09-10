using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/users/{id}")]
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

    // POST api/users/{id}/follow
    [HttpPost("follow")]
    public async Task<IActionResult> Follow(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var (succeeded, error) = await _followService.FollowAsync(currentUserId, id);
        return succeeded ? NoContent() : BadRequest(error);
    }

    // DELETE api/users/{id}/follow
    [HttpDelete("follow")]
    public async Task<IActionResult> Unfollow(string id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var (succeeded, error) = await _followService.UnfollowAsync(currentUserId, id);
        return succeeded ? NoContent() : BadRequest(error);
    }

    // GET api/users/{id}/following?page=1&pageSize=20
    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _followService.GetFollowingAsync(id, currentUserId, page, pageSize);
        return Ok(result);
    }

    // GET api/users/{id}/followers?page=1&pageSize=20
    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _followService.GetFollowersAsync(id, currentUserId, page, pageSize);
        return Ok(result);
    }
}