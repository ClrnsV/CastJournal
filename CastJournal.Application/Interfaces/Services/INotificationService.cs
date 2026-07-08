using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Notifications;
using CastJournal.Domain.Enums;


namespace CastJournal.Application.Interfaces.Services;

public interface INotificationService
{
    // Called from anywhere else in the app whenever something notification-worthy happens
    Task CreateNotificationAsync(string userId, NotificationType type, string title, string message,
        string? relatedEntityType = null, string? relatedEntityId = null);

    Task<PagedResult<NotificationDto>> GetNotificationsAsync(string userId, NotificationFilterDto filter);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(Guid id, string userId);
    Task MarkAllAsReadAsync(string userId);
}