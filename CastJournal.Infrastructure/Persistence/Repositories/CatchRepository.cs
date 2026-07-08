using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Infrastructure.Persistence.Repositories;

public class CatchRepository : ICatchRepository
{
    private readonly ApplicationDbContext _context;

    public CatchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Catch?> GetByIdAsync(Guid id)
    {
        return await _context.Catches
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Include(c => c.Media)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Catch>> GetAllByUserIdAsync(string userId)
    {
        return await _context.Catches
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Include(c => c.Media)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CatchDate)
            .ToListAsync();
    }

    public async Task AddAsync(Catch entity)
    {
        await _context.Catches.AddAsync(entity);
    }

    public void Update(Catch entity)
    {
        _context.Catches.Update(entity);
    }

    public void Delete(Catch entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = "current-user-id";   //pass userId later
        _context.Catches.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddMediaAsync(CatchMedia media)
    {
        await _context.CatchMedia.AddAsync(media);
    }

    public async Task<CatchMedia?> GetMediaByIdAsync(Guid mediaId)
    {
        return await _context.CatchMedia
            .Include(m => m.Catch)
            .FirstOrDefaultAsync(m => m.Id == mediaId);
    }

    public void DeleteMedia(CatchMedia media)
    {
        _context.CatchMedia.Remove(media);
    }

    public async Task<(IEnumerable<Catch> Items, int TotalCount)> GetFilteredByUserIdAsync(string userId, CatchFilterDto filter)
    {
        var query = _context.Catches
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Include(c => c.Media)
            .Where(c => c.UserId == userId);

        if (filter.SpeciesId.HasValue)
            query = query.Where(c => c.SpeciesId == filter.SpeciesId.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(c => c.LocationId == filter.LocationId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(c => c.CatchDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(c => c.CatchDate <= filter.EndDate.Value);

        if (filter.MinWeight.HasValue)
            query = query.Where(c => c.Weight >= filter.MinWeight.Value);

        if (filter.MaxWeight.HasValue)
            query = query.Where(c => c.Weight <= filter.MaxWeight.Value);

        if (!string.IsNullOrWhiteSpace(filter.GearUsed))
            query = query.Where(c => c.GearUsed != null && c.GearUsed.Contains(filter.GearUsed));

        if (!string.IsNullOrWhiteSpace(filter.BaitUsed))
            query = query.Where(c => c.BaitUsed != null && c.BaitUsed.Contains(filter.BaitUsed));

        if (!string.IsNullOrWhiteSpace(filter.FishingMethod))
            query = query.Where(c => c.FishingMethod != null && c.FishingMethod.Contains(filter.FishingMethod));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c =>
                (c.Notes != null && c.Notes.Contains(term)) ||
                (c.GearUsed != null && c.GearUsed.Contains(term)) ||
                (c.BaitUsed != null && c.BaitUsed.Contains(term)) ||
                (c.FishingMethod != null && c.FishingMethod.Contains(term)) ||
                (c.Species != null && c.Species.CommonName.Contains(term)) ||
                (c.Location != null && c.Location.Name.Contains(term)));
        }

        query = filter.SortBy?.ToLower() switch
        {
            "weight" => filter.SortDescending == false
                ? query.OrderBy(c => c.Weight)
                : query.OrderByDescending(c => c.Weight),
            "length" => filter.SortDescending == false
                ? query.OrderBy(c => c.Length)
                : query.OrderByDescending(c => c.Length),
            _ => filter.SortDescending == false
                ? query.OrderBy(c => c.CatchDate)
                : query.OrderByDescending(c => c.CatchDate)
        };

        var totalCount = await query.CountAsync();

        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 20;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<(IEnumerable<Catch> Items, int TotalCount)> GetPublicFeedAsync(string? userId, List<string>? followingIds, int page, int pageSize)
    {
        var query = _context.Catches
            .Include(c => c.User)
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Include(c => c.Media)
            .Where(c => c.IsPublic);

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(c => c.UserId == userId);

        if (followingIds != null)
            query = query.Where(c => followingIds.Contains(c.UserId));

        query = query.OrderByDescending(c => c.CatchDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
    public async Task<List<Catch>> GetForAnalyticsAsync(string userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Catches
            .AsNoTracking()
            .Include(c => c.Species)
            .Include(c => c.Location)
            .Where(c => c.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(c => c.CatchDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.CatchDate <= endDate.Value);

        return await query.ToListAsync();
    }
    public async Task<bool> SpeciesExistsAsync(Guid speciesId)
    {
        return await _context.Species.AnyAsync(s => s.Id == speciesId);
    }

    public async Task<bool> LocationExistsAsync(Guid locationId)
    {
        return await _context.FishingLocations.AnyAsync(l => l.Id == locationId);
    }
    public async Task<int> CountPublicCatchesByUserIdAsync(string userId)
    {
        return await _context.Catches
            .CountAsync(c => c.UserId == userId && c.IsPublic);
    }
}
