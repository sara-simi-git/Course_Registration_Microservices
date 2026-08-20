using NotificationService.Models;

namespace NotificationService.Interfaces.IRepositories;

public interface INotificationRepository
{
    Task SaveAsync(Notification notification);
    Task<Notification?> GetByIdAsync(string id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(string id);
}
