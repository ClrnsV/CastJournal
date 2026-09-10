using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Follow;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CastJournal.Application.Services;
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly UserManager<User> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public FollowService(IFollowRepository followRepository, UserManager<User> userManager,
        INotificationService notificationService, IMapper mapper)
    {
        _followRepository = followRepository;
        _userManager = userManager;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<(bool Succeeded, string? Error)> FollowAsync(string followerId, string followingId)
    {
        if (followerId == followingId)
            return (false, "You cannot follow yourself.");

        var targetUser = await _userManager.FindByIdAsync(followingId);
        if (targetUser == null)
            return (false, "User not found.");

        if (await _followRepository.ExistsAsync(followerId, followingId))
            return (true, null);

        await _followRepository.FollowAsync(followerId, followingId);

        var follower = await _userManager.FindByIdAsync(followerId);
        var followerName = follower?.FullName ?? follower?.UserName ?? "Someone";

        await _notificationService.CreateNotificationAsync(
            followingId,
            NotificationType.NewFollower,
            "New Follower",
            $"{followerName} started following you!",
            relatedEntityType: "User",
            relatedEntityId: followerId);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UnfollowAsync(string followerId, string followingId)
    {
        // Idempotent — unfollowing someone you don't follow is a no-op success, not an error.
        await _followRepository.UnfollowAsync(followerId, followingId);
        return (true, null);
    }

    public async Task<PagedResult<FollowUserDto>> GetFollowersAsync(string userId, string? currentUserId, int page, int pageSize)
    {
        var (users, totalCount) = await _followRepository.GetFollowersAsync(userId, page, pageSize);
        var items = await MapWithFollowStatusAsync(users, currentUserId);

        return new PagedResult<FollowUserDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<FollowUserDto>> GetFollowingAsync(string userId, string? currentUserId, int page, int pageSize)
    {
        var (users, totalCount) = await _followRepository.GetFollowingAsync(userId, page, pageSize);
        var items = await MapWithFollowStatusAsync(users, currentUserId);

        return new PagedResult<FollowUserDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private async Task<List<FollowUserDto>> MapWithFollowStatusAsync(List<User> users, string? currentUserId)
    {
        var dtos = users.Select(u => _mapper.Map<FollowUserDto>(u)).ToList();

        if (!string.IsNullOrEmpty(currentUserId) && dtos.Count > 0)
        {
            var followedIds = await _followRepository.GetFollowingIdsAmongAsync(
                currentUserId, users.Select(u => u.Id).ToList());
            var followedSet = followedIds.ToHashSet();

            foreach (var dto in dtos)
                dto.IsFollowedByCurrentUser = followedSet.Contains(dto.Id);
        }

        return dtos;
    }
}