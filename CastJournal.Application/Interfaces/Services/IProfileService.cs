using CastJournal.Application.DTOs.Profile;
using Microsoft.AspNetCore.Http;
namespace CastJournal.Application.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(string userId);
    Task<ProfileDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<ProfileDto?> UploadAvatarAsync(string userId, IFormFile file);
    Task<PublicProfileDto?> GetPublicProfileAsync(string userId);

}