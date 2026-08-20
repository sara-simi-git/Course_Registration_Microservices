using System.Text.Json;
using NotificationService.Data;
using NotificationService.Interfaces.IRepositories;
using NotificationService.Models;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly RedisContext _context;
    private const string KeyPrefix = "notification:";

    public NotificationRepository(RedisContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Notification notification)
    {
        var db = _context.GetDatabase();
        var json = JsonSerializer.Serialize(notification);
        await db.StringSetAsync($"{KeyPrefix}{notification.Id}", json);
        await db.ListLeftPushAsync($"user:{notification.UserId}:notifications", notification.Id);
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        var db = _context.GetDatabase();
        var json = await db.StringGetAsync($"{KeyPrefix}{id}");
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Notification>(json!);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        var db = _context.GetDatabase();
        var ids = await db.ListRangeAsync($"user:{userId}:notifications");
        var results = new List<Notification>();
        foreach (var id in ids)
        {
            var n = await GetByIdAsync(id!);
            if (n != null) results.Add(n);
        }
        return results;
    }

    public async Task UpdateAsync(Notification notification)
    {
        var db = _context.GetDatabase();
        var json = JsonSerializer.Serialize(notification);
        await db.StringSetAsync($"{KeyPrefix}{notification.Id}", json);
    }

    public async Task DeleteAsync(string id)
    {
        var db = _context.GetDatabase();
        await db.KeyDeleteAsync($"{KeyPrefix}{id}");
    }
}
