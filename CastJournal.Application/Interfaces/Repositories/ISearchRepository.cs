using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;
public interface ISearchRepository
{
    Task<(List<User> Users, int TotalCount)> SearchUsersAsync(string query, int page, int pageSize);
    Task<(List<Catch> Catches, int TotalCount)> SearchPublicCatchesAsync(string query, string? currentUserId, int page, int pageSize);
}