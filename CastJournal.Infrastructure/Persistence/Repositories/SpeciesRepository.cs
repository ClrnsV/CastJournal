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

public class SpeciesRepository : ISpeciesRepository
{
    private readonly ApplicationDbContext _context;

    public SpeciesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Species?> GetByIdAsync(Guid id)
    {
        return await _context.Species.FindAsync(id);
    }

    public async Task<IEnumerable<Species>> GetAllAsync(bool onlyApproved = true)
    {
        var query = _context.Species.AsQueryable();

        if (onlyApproved)
            query = query.Where(s => s.IsApproved);

        return await query.OrderBy(s => s.CommonName).ToListAsync();
    }

    public async Task AddAsync(Species species)
    {
        await _context.Species.AddAsync(species);
    }

    public void Update(Species species)
    {
        _context.Species.Update(species);
    }

    public void Delete(Species species)
    {
        _context.Species.Remove(species);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}