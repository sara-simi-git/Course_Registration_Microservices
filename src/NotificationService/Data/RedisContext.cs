using StackExchange.Redis;

namespace NotificationService.Data;

public class RedisContext
{
    private readonly IConnectionMultiplexer _redis;

    public RedisContext(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public IDatabase GetDatabase() => _redis.GetDatabase();
}
