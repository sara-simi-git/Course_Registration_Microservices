using NotificationService.DTOs;
using NotificationService.Enums;
using NotificationService.Interfaces.IRepositories;
using NotificationService.Interfaces.IServices;
using NotificationService.Models;

namespace NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository repository, ILogger<NotificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<NotificationResponseDto> SendNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            CourseId = dto.CourseId,
            Type = dto.Type,
            Message = dto.Message,
            Status = NotificationStatus.Pending
        };

        await _repository.SaveAsync(notification);

        try
        {
            _logger.LogInformation("Sending notification to user {UserId}: {Message}", dto.UserId, dto.Message);
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            notification.Status = NotificationStatus.Failed;
        }

        await _repository.UpdateAsync(notification);
        return ToDto(notification);
    }

    public async Task<NotificationResponseDto?> GetByIdAsync(string id)
    {
        var n = await _repository.GetByIdAsync(id);
        return n == null ? null : ToDto(n);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(string userId)
    {
        var list = await _repository.GetByUserIdAsync(userId);
        return list.Select(ToDto);
    }

    public async Task RetryFailedAsync(string id)
    {
        var notification = await _repository.GetByIdAsync(id);
        if (notification == null) return;

        notification.RetryCount++;
        notification.Status = NotificationStatus.Retrying;
        _logger.LogInformation("Retrying notification {Id}, attempt {Count}", id, notification.RetryCount);

        notification.Status = NotificationStatus.Sent;
        notification.SentAt = DateTime.UtcNow;
        await _repository.UpdateAsync(notification);
    }

    private static NotificationResponseDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        CourseId = n.CourseId,
        Type = n.Type,
        Message = n.Message,
        Status = n.Status.ToString(),
        CreatedAt = n.CreatedAt,
        SentAt = n.SentAt,
        RetryCount = n.RetryCount
    };
}
