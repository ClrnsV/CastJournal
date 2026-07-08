using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;



namespace CastJournal.Infrastructure.Persistence.Repositories;
public class FollowRepository : IFollowRepository
{
    private readonly ApplicationDbContext _context;

    public FollowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string followerId, string followingId)
    {
        return await _context.UserFollows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task FollowAsync(string followerId, string followingId)
    {
        _context.UserFollows.Add(new UserFollow
        {
            FollowerId = followerId,
            FollowingId = followingId
        });
        await _context.SaveChangesAsync();
    }

    public async Task UnfollowAsync(string followerId, string followingId)
    {
        var existing = await _context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (existing != null)
        {
            _context.UserFollows.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetFollowerCountAsync(string userId)
    {
        return await _context.UserFollows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _context.UserFollows.CountAsync(f => f.FollowerId == userId);
    }

    public async Task<List<string>> GetFollowingIdsAsync(string followerId)
    {
        return await _context.UserFollows
            .Where(f => f.FollowerId == followerId)
            .Select(f => f.FollowingId)
            .ToListAsync();
    }
}