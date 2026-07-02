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
        _context.Catches.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
