using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using NotificationService.Interfaces.IServices;

namespace NotificationService.Consumers;

public class OrderFinalConsumer : BackgroundService
{
    private readonly ILogger<OrderFinalConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderFinalConsumer(ILogger<OrderFinalConsumer> logger, IConfiguration config, IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _config = config;
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = _config["RabbitMQ:Host"] ?? "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        var exchange = _config["RabbitMQ:Exchange"] ?? "learning";
        _channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true, autoDelete: false);

        var queueName = "notification.order.final";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, exchange, "OrderConfirmed");
        _channel.QueueBind(queueName, exchange, "OrderCanceled");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
            var correlationId = ea.BasicProperties?.CorrelationId ?? Guid.NewGuid().ToString();

            var db = _redis.GetDatabase();
            var processedKey = $"processed:{messageId}";
            var acquired = await db.StringSetAsync(processedKey, "1", TimeSpan.FromHours(1), when: When.NotExists);
            if (!acquired)
            {
                _logger.LogInformation("Duplicate final order message ignored: {MessageId}", messageId);
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }

            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            if (ea.RoutingKey == "OrderConfirmed")
            {
                var evt = JsonSerializer.Deserialize<OrderConfirmed>(body);
                if (evt != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await service.SendNotificationAsync(new DTOs.CreateNotificationDto
                    {
                        UserId = evt.RegistrationId.ToString(),
                        CourseId = evt.CourseId.ToString(),
                        Type = "Confirmed",
                        Message = $"Order {evt.RegistrationId} confirmed"
                    });
                    _logger.LogInformation("OrderConfirmed processed for {RegistrationId} CorrelationId={CorrelationId}", evt.RegistrationId, correlationId);
                }
            }
            else if (ea.RoutingKey == "OrderCanceled")
            {
                var evt = JsonSerializer.Deserialize<OrderCanceled>(body);
                if (evt != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await service.SendNotificationAsync(new DTOs.CreateNotificationDto
                    {
                        UserId = evt.RegistrationId.ToString(),
                        CourseId = evt.CourseId.ToString(),
                        Type = "Canceled",
                        Message = $"Order {evt.RegistrationId} canceled"
                    });
                    _logger.LogInformation("OrderCanceled processed for {RegistrationId} CorrelationId={CorrelationId}", evt.RegistrationId, correlationId);
                }
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private record OrderConfirmed(int RegistrationId, Guid CourseId, DateTime ConfirmedAt, string CorrelationId);
    private record OrderCanceled(int RegistrationId, Guid CourseId, DateTime CanceledAt, string CorrelationId);
}
