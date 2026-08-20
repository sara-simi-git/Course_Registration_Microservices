using System.Text;
using System.Text.Json;
using InventoryService.DTOs;
using InventoryService.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace InventoryService.Consumers;

public class OrderPlacedConsumer : BackgroundService
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IModel? _channel;

    public OrderPlacedConsumer(
        ILogger<OrderPlacedConsumer> logger,
        IConfiguration config,
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory)
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
        _channel.ExchangeDeclare(exchange, ExchangeType.Direct, true, false);

        var queueName = "inventory.order.placed";
        _channel.QueueDeclare(queueName, true, false, false);
        _channel.QueueBind(queueName, exchange, "OrderPlaced");
        _channel.QueueBind(queueName, exchange, "OrderCanceled");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
                var correlationId = ea.BasicProperties?.CorrelationId ?? Guid.NewGuid().ToString();

                var redisDb = _redis.GetDatabase();
                if (!await redisDb.StringSetAsync($"processed:{messageId}", "1",
                        TimeSpan.FromHours(1), When.NotExists))
                {
                    _logger.LogInformation("Duplicate message {MessageId}", messageId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _scopeFactory.CreateScope();
                var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                if (ea.RoutingKey == "OrderPlaced")
                {
                    var order = JsonSerializer.Deserialize<OrderPlaced>(body);
                    if (order == null)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    _logger.LogInformation("Received OrderPlaced RegistrationId={RegistrationId} CorrelationId={CorrelationId}", order.RegistrationId, correlationId);

                    var reserved = await inventoryService.ReserveSeatAsync(
                        new SeatOperationDto { CourseId = order.CourseId });

                    var evt = reserved
                        ? JsonSerializer.SerializeToUtf8Bytes(new InventoryReserved(order.RegistrationId, order.CourseId, DateTime.UtcNow, correlationId))
                        : JsonSerializer.SerializeToUtf8Bytes(new InventoryRejected(order.RegistrationId, order.CourseId, DateTime.UtcNow, correlationId));

                    _logger.LogInformation("Inventory outcome for RegistrationId={RegistrationId} Reserved={Reserved} CorrelationId={CorrelationId}", order.RegistrationId, reserved, correlationId);

                    var props = _channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.MessageId = Guid.NewGuid().ToString();
                    props.CorrelationId = correlationId;

                    _channel.BasicPublish(exchange,
                        reserved ? "InventoryReserved" : "InventoryRejected",
                        props,
                        evt);
                }
                else if (ea.RoutingKey == "OrderCanceled")
                {
                    var cancel = JsonSerializer.Deserialize<OrderCanceled>(body);
                    if (cancel != null)
                    {
                        await inventoryService.ReleaseSeatAsync(
                            new SeatOperationDto { CourseId = cancel.CourseId });
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer error");
                _channel?.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queueName, false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private record OrderPlaced(int RegistrationId, int EmployeeId, Guid CourseId, DateTime PlacedAt, string CorrelationId);
    private record InventoryReserved(int RegistrationId, Guid CourseId, DateTime ReservedAt, string CorrelationId);
    private record InventoryRejected(int RegistrationId, Guid CourseId, DateTime RejectedAt, string CorrelationId);
    private record OrderCanceled(int RegistrationId, Guid CourseId, DateTime CanceledAt, string CorrelationId);
}
