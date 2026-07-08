using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Persistence.Repositories;
public class LikeRepository : ILikeRepository
{
    private readonly ApplicationDbContext _context;

    public LikeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid catchId, string userId)
    {
        return await _context.CatchLikes.AnyAsync(l => l.CatchId == catchId && l.UserId == userId);
    }

    public async Task LikeAsync(Guid catchId, string userId)
    {
        _context.CatchLikes.Add(new CatchLike { CatchId = catchId, UserId = userId });
        await _context.SaveChangesAsync();
    }

    public async Task UnlikeAsync(Guid catchId, string userId)
    {
        var like = await _context.CatchLikes.FirstOrDefaultAsync(l => l.CatchId == catchId && l.UserId == userId);
        if (like != null)
        {
            _context.CatchLikes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetLikeCountAsync(Guid catchId)
    {
        return await _context.CatchLikes.CountAsync(l => l.CatchId == catchId);
    }

    public async Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> catchIds)
    {
        return await _context.CatchLikes
            .Where(l => catchIds.Contains(l.CatchId))
            .GroupBy(l => l.CatchId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }
}