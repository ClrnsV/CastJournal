using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using CastJournal.Domain.Entities;



namespace CastJournal.Application.Services;
public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly UserManager<User> _userManager;

    public FollowService(IFollowRepository followRepository, UserManager<User> userManager)
    {
        _followRepository = followRepository;
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? Error)> FollowAsync(string followerId, string followingId)
    {
        if (followerId == followingId)
            return (false, "You cannot follow yourself.");

        var targetUser = await _userManager.FindByIdAsync(followingId);
        if (targetUser == null)
            return (false, "User not found.");

        // Idempotent — following someone you already follow is a no-op success, not an error.
        if (await _followRepository.ExistsAsync(followerId, followingId))
            return (true, null);

        await _followRepository.FollowAsync(followerId, followingId);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UnfollowAsync(string followerId, string followingId)
    {
        // Idempotent — unfollowing someone you don't follow is a no-op success, not an error.
        await _followRepository.UnfollowAsync(followerId, followingId);
        return (true, null);
    }
}