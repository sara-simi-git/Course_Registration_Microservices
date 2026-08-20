namespace NotificationService.DTOs;

public class CreateNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
}
//fix
public class RegistrationCreatedEvent
{
    public string RegistrationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
