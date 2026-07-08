using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/catches/{id}/like")]
[ApiController]
[Authorize]
public class LikeController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikeController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpPost]
    public async Task<IActionResult> Like(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (succeeded, error) = await _likeService.LikeAsync(id, userId);
        return succeeded ? NoContent() : BadRequest(error);
    }

    [HttpDelete]
    public async Task<IActionResult> Unlike(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (succeeded, error) = await _likeService.UnlikeAsync(id, userId);
        return succeeded ? NoContent() : BadRequest(error);
    }
}