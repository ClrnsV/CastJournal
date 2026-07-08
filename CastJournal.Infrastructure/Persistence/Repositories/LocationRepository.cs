using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Persistence.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;

    public LocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FishingLocation?> GetByIdAsync(Guid id)
    {
        return await _context.FishingLocations.FindAsync(id);
    }

    public async Task<IEnumerable<FishingLocation>> GetAllByUserIdAsync(string userId)
    {
        return await _context.FishingLocations
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }
    public async Task<IEnumerable<FishingLocation>> GetPublicLocationsAsync()
    {
        return await _context.FishingLocations
            .Where(l => l.IsPublic)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task AddAsync(FishingLocation location)
    {
        await _context.FishingLocations.AddAsync(location);
    }

    public void Update(FishingLocation location)
    {
        _context.FishingLocations.Update(location);
    }

    public void Delete(FishingLocation location, string deletedBy)
    {
        location.IsDeleted = true;
        location.DeletedAt = DateTime.UtcNow;
        location.DeletedBy = deletedBy;
        _context.FishingLocations.Update(location);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}