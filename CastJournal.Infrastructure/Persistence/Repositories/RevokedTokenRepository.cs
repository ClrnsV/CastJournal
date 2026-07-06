using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Persistence.Repositories;

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RevokedTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RevokedToken token)
    {
        _context.RevokedTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRevokedAsync(string jti)
    {
        return await _context.RevokedTokens.AnyAsync(r => r.Jti == jti);
    }
}