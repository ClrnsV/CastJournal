using CastJournal.Application.DTOs;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CastJournal.API.Controllers;

[Route("api/media")]
[ApiController]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly ICatchService _catchService;
    public MediaController(ICatchService catchService)
    {
        _catchService = catchService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia([FromForm] Guid catchId, IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var success = await _catchService.UploadMediaAsync(catchId, file, userId);
        if (!success)
            return BadRequest("Failed to upload media or unauthorized");

        return Ok(new { message = "Media uploaded successfully" });
    }

    [HttpGet("catch/{catchId}")]
    public async Task<ActionResult<IEnumerable<CatchMediaDto>>> GetCatchMedia(Guid catchId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var media = await _catchService.GetCatchMediaAsync(catchId, userId);
        return Ok(media);
    }

    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> DeleteMedia(Guid mediaId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var success = await _catchService.DeleteMediaAsync(mediaId, userId);
        if (!success)
            return NotFound("Media not found or unauthorized");

        return NoContent();
    }
}