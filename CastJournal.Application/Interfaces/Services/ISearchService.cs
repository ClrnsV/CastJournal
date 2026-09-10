using CastJournal.Application.DTOs.Search;

namespace CastJournal.Application.Interfaces.Services;
public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string query, string type, string? currentUserId, int page, int pageSize);
}