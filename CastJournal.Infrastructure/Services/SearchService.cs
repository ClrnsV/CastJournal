using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.DTOs.Search;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;

namespace CastJournal.Application.Services;
public class SearchService : ISearchService
{
    // "All" mode gives a taste of each type, not full pages — matches Facebook's
    // combined-view behavior. Switching to a specific tab re-queries with real paging.
    private const int PreviewSize = 5;

    private readonly ISearchRepository _searchRepository;
    private readonly IFollowRepository _followRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly ICommentRepository _commentRepository;   // NEW
    private readonly IMapper _mapper;

    public SearchService(ISearchRepository searchRepository, IFollowRepository followRepository,
        ILikeRepository likeRepository, ICommentRepository commentRepository, IMapper mapper)
    {
        _searchRepository = searchRepository;
        _followRepository = followRepository;
        _likeRepository = likeRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<SearchResultDto> SearchAsync(string query, string type, string? currentUserId, int page, int pageSize)
    {
        var result = new SearchResultDto();
        var normalizedType = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();

        if (normalizedType == "all" || normalizedType == "users")
        {
            var (p, size) = normalizedType == "all" ? (1, PreviewSize) : (page, pageSize);
            result.Users = await SearchUsersAsync(query, currentUserId, p, size);
        }

        if (normalizedType == "all" || normalizedType == "catches")
        {
            var (p, size) = normalizedType == "all" ? (1, PreviewSize) : (page, pageSize);
            result.Catches = await SearchCatchesAsync(query, currentUserId, p, size);
        }

        return result;
    }

    private async Task<PagedResult<UserSearchResultDto>> SearchUsersAsync(string query, string? currentUserId, int page, int pageSize)
    {
        var (users, totalCount) = await _searchRepository.SearchUsersAsync(query, page, pageSize);
        var dtos = users.Select(u => _mapper.Map<UserSearchResultDto>(u)).ToList();

        if (!string.IsNullOrEmpty(currentUserId) && dtos.Count > 0)
        {
            var followedIds = await _followRepository.GetFollowingIdsAmongAsync(
                currentUserId, users.Select(u => u.Id).ToList());
            var followedSet = followedIds.ToHashSet();

            foreach (var dto in dtos)
                dto.IsFollowedByCurrentUser = followedSet.Contains(dto.Id);
        }

        return new PagedResult<UserSearchResultDto> { Items = dtos, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private async Task<PagedResult<CatchDto>> SearchCatchesAsync(string query, string? currentUserId, int page, int pageSize)
    {
        var (catches, totalCount) = await _searchRepository.SearchPublicCatchesAsync(query, currentUserId, page, pageSize);
        var dtos = _mapper.Map<List<CatchDto>>(catches);

        await PopulateLikeDataAsync(dtos, currentUserId);

        return new PagedResult<CatchDto> { Items = dtos, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    // Mirrors CatchService.PopulateLikeDataAsync exactly — same batched approach,
    // avoiding an N+1 query across the search results.
    private async Task PopulateLikeDataAsync(List<CatchDto> dtos, string? currentUserId)
    {
        if (dtos.Count == 0) return;

        var catchIds = dtos.Select(d => d.Id).ToList();

        var likeCounts = await _likeRepository.GetLikeCountsAsync(catchIds);
        foreach (var dto in dtos)
            dto.LikeCount = likeCounts.GetValueOrDefault(dto.Id, 0);

        var commentCounts = await _commentRepository.GetCommentCountsAsync(catchIds);
        foreach (var dto in dtos)
            dto.CommentCount = commentCounts.GetValueOrDefault(dto.Id, 0);  

        if (string.IsNullOrEmpty(currentUserId)) return;

        var likedIds = await _likeRepository.GetLikedCatchIdsAsync(catchIds, currentUserId);
        foreach (var dto in dtos)
            dto.IsLikedByCurrentUser = likedIds.Contains(dto.Id);
    }
}