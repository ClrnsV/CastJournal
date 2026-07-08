using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Notifications;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;


namespace CastJournal.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task CreateNotificationAsync(string userId, NotificationType type, string title, string message,
        string? relatedEntityType = null, string? relatedEntityId = null)
    {
        await _notificationRepository.AddAsync(new Notification
        {
            UserId = userId,
            Type = type.ToString(),
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        });
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(string userId, NotificationFilterDto filter)
    {
        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 20;

        var (items, totalCount) = await _notificationRepository.GetByUserIdAsync(userId, filter.IsRead, page, pageSize);

        return new PagedResult<NotificationDto>
        {
            Items = _mapper.Map<IEnumerable<NotificationDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, string userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.SaveChangesAsync();
        }

        return true;
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }
}