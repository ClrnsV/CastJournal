using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Follow;



namespace CastJournal.Application.Interfaces.Services;
public interface IFollowService
{
    Task<(bool Succeeded, string? Error)> FollowAsync(string followerId, string followingId);
    Task<(bool Succeeded, string? Error)> UnfollowAsync(string followerId, string followingId);
    Task<PagedResult<FollowUserDto>> GetFollowersAsync(string userId, string? currentUserId, int page, int pageSize);
    Task<PagedResult<FollowUserDto>> GetFollowingAsync(string userId, string? currentUserId, int page, int pageSize);
}