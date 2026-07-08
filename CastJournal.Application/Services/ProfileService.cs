using AutoMapper;
using CastJournal.Application.DTOs.Profile;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
namespace CastJournal.Application.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICatchRepository _catchRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IMapper _mapper;

    public ProfileService(UserManager<User> userManager, IFileStorageService fileStorageService,
     ICatchRepository catchRepository, IFollowRepository followRepository, IMapper mapper)
    {
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _catchRepository = catchRepository;
        _followRepository = followRepository;
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
    public async Task<PublicProfileDto?> GetPublicProfileAsync(string userId, string? currentUserId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var publicProfile = _mapper.Map<PublicProfileDto>(user);
        publicProfile.PublicCatchCount = await _catchRepository.CountPublicCatchesByUserIdAsync(userId);
        publicProfile.FollowerCount = await _followRepository.GetFollowerCountAsync(userId);
        publicProfile.FollowingCount = await _followRepository.GetFollowingCountAsync(userId);

        if (!string.IsNullOrEmpty(currentUserId))
            publicProfile.IsFollowedByCurrentUser = await _followRepository.ExistsAsync(currentUserId, userId);

        return publicProfile;
    }
}