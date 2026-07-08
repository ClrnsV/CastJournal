using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;

namespace CastJournal.Application.Services;
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly ICatchRepository _catchRepository;

    public LikeService(ILikeRepository likeRepository, ICatchRepository catchRepository)
    {
        _likeRepository = likeRepository;
        _catchRepository = catchRepository;
    }

    public async Task<(bool Succeeded, string? Error)> LikeAsync(Guid catchId, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(catchId);
        if (catchEntity == null)
            return (false, "Catch not found.");

        // Idempotent — liking something you already liked is a no-op success, not an error.
        if (await _likeRepository.ExistsAsync(catchId, userId))
            return (true, null);

        await _likeRepository.LikeAsync(catchId, userId);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UnlikeAsync(Guid catchId, string userId)
    {
        // Idempotent — unliking something you haven't liked is a no-op success, not an error.
        await _likeRepository.UnlikeAsync(catchId, userId);
        return (true, null);
    }
}