using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string hashedToken);
    Task UpdateAsync(RefreshToken token);
}