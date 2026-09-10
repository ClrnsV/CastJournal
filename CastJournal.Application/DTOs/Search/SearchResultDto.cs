using CastJournal.Application.DTOs.Catches;

namespace CastJournal.Application.DTOs.Search;
public class SearchResultDto
{
    public PagedResult<UserSearchResultDto> Users { get; set; } = new();
    public PagedResult<CatchDto> Catches { get; set; } = new();
}