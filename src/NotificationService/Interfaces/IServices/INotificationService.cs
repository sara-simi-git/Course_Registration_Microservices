using NotificationService.DTOs;

namespace NotificationService.Interfaces.IServices;

public interface INotificationService
{
    Task<NotificationResponseDto> SendNotificationAsync(CreateNotificationDto dto);
    Task<NotificationResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(string userId);
    Task RetryFailedAsync(string id);
}
