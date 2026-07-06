using CastJournal.Application.DTOs.Profile;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CastJournal.API.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var profile = await _profileService.UpdateProfileAsync(userId, dto);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ProfileDto>> UploadAvatar(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var profile = await _profileService.UploadAvatarAsync(userId, file);
        if (profile == null) return NotFound();

        return Ok(profile);
    }
}