using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Persistence.Repositories;
public class SearchRepository : ISearchRepository
{
    private readonly ApplicationDbContext _context;

    public SearchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<User> Users, int TotalCount)> SearchUsersAsync(string query, int page, int pageSize)
    {
        var term = query.Trim();

        var baseQuery = _context.Users.Where(u => u.IsActive &&
            ((u.FullName != null && u.FullName.Contains(term)) ||
             (u.UserName != null && u.UserName.Contains(term))));

        var totalCount = await baseQuery.CountAsync();
        var users = await baseQuery
            .OrderBy(u => u.FullName ?? u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<(List<Catch> Catches, int TotalCount)> SearchPublicCatchesAsync(string query, string? currentUserId, int page, int pageSize)
    {
        var term = query.Trim();

        // Public catches from anyone, plus the searching user's own — even if unpublished —
        // mirrors how their normal /catches/search already lets them find their own stuff.
        var baseQuery = _context.Catches
            .Include(c => c.User)
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Include(c => c.Media)
            .Where(c => c.IsPublic || c.UserId == currentUserId)
            .Where(c =>
                (c.Notes != null && c.Notes.Contains(term)) ||
                (c.GearUsed != null && c.GearUsed.Contains(term)) ||
                (c.BaitUsed != null && c.BaitUsed.Contains(term)) ||
                (c.FishingMethod != null && c.FishingMethod.Contains(term)) ||
                (c.Species != null && c.Species.CommonName.Contains(term)) ||
                (c.Location != null && c.Location.Name.Contains(term)));

        var totalCount = await baseQuery.CountAsync();
        var catches = await baseQuery
            .OrderByDescending(c => c.CatchDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (catches, totalCount);
    }
}