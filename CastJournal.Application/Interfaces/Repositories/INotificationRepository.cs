using CastJournal.Domain.Entities;


namespace CastJournal.Application.Interfaces.Repositories;
public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<(IEnumerable<Notification> Items, int TotalCount)> GetByUserIdAsync(string userId, bool? isRead, int page, int pageSize);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task MarkAllAsReadAsync(string userId);
    Task SaveChangesAsync();
}