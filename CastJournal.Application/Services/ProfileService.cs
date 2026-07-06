using AutoMapper;
using CastJournal.Application.DTOs.Profile;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
namespace CastJournal.Application.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public ProfileService(UserManager<User> userManager, IFileStorageService fileStorageService, IMapper mapper)
    {
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<ProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return _mapper.Map<ProfileDto>(user);
    }

    public async Task<ProfileDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        user.FullName = dto.FullName ?? user.FullName;
        user.Bio = dto.Bio ?? user.Bio;
        user.PreferredFishingMethods = dto.PreferredFishingMethods ?? user.PreferredFishingMethods;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        return _mapper.Map<ProfileDto>(user);
    }

    public async Task<ProfileDto?> UploadAvatarAsync(string userId, IFormFile file)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(user.AvatarUrl))
            await _fileStorageService.DeleteFileAsync(user.AvatarUrl);   // clean up old avatar

        user.AvatarUrl = await _fileStorageService.SaveAvatarAsync(file, userId);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        return _mapper.Map<ProfileDto>(user);
    }
}