using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CastJournal.Application.Services;
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly ICatchRepository _catchRepository;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;

    public LikeService(ILikeRepository likeRepository, ICatchRepository catchRepository,
        INotificationService notificationService, UserManager<User> userManager)
    {
        _likeRepository = likeRepository;
        _catchRepository = catchRepository;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? Error)> LikeAsync(Guid catchId, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(catchId);
        if (catchEntity == null)
            return (false, "Catch not found.");

        if (await _likeRepository.ExistsAsync(catchId, userId))
            return (true, null);

        await _likeRepository.LikeAsync(catchId, userId);

        if (catchEntity.UserId != userId)
        {
            var liker = await _userManager.FindByIdAsync(userId);
            var likerName = liker?.FullName ?? liker?.UserName ?? "Someone";

            await _notificationService.CreateNotificationAsync(
                catchEntity.UserId,
                NotificationType.CatchLiked,
                "New Like",
                $"{likerName} liked your catch!",
                relatedEntityType: "Catch",
                relatedEntityId: catchId.ToString());
        }

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UnlikeAsync(Guid catchId, string userId)
    {
        await _likeRepository.UnlikeAsync(catchId, userId);
        return (true, null);
    }
}