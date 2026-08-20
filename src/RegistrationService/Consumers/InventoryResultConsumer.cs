using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using RegistrationService.Interfaces;
using RegistrationService.Repositories;
using RegistrationService.Services;

namespace RegistrationService.Consumers;

public class InventoryResultConsumer : BackgroundService
{
    private readonly ILogger<InventoryResultConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IModel? _channel;

    public InventoryResultConsumer(ILogger<InventoryResultConsumer> logger, IConfiguration config, IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
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

        var queueName = "registration.inventory.results";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, exchange, "InventoryReserved");
        _channel.QueueBind(queueName, exchange, "InventoryRejected");

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
                _logger.LogInformation("Duplicate inventory result ignored: {MessageId}", messageId);
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }

            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            // try deserialize as InventoryReserved first
            try
            {
                if (ea.RoutingKey == "InventoryReserved")
                {
                    var evt = JsonSerializer.Deserialize<InventoryReserved>(body);
                    if (evt != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<ICourseRegistrationRepository>();

                        _logger.LogInformation("InventoryReserved received for {RegistrationId} CorrelationId={CorrelationId}", evt.RegistrationId, correlationId);
                        // confirm order — mark status to Active (already Active) but we may persist
                        await repo.ChangeStatusCourseAsync(evt.RegistrationId, RegistrationService.Enums.CourseRegistrationStatus.Active);

                        // publish OrderConfirmed
                        using var connOut = factory.CreateConnection();
                        using var chOut = connOut.CreateModel();
                        var outEvt = new OrderConfirmed(evt.RegistrationId, evt.CourseId, DateTime.UtcNow, correlationId);
                        var bytes = JsonSerializer.SerializeToUtf8Bytes(outEvt);
                        var props = chOut.CreateBasicProperties();
                        props.Persistent = true;
                        props.MessageId = Guid.NewGuid().ToString();
                        props.CorrelationId = correlationId;
                        chOut.BasicPublish(exchange, "OrderConfirmed", props, bytes);
                    }
                }
                else if (ea.RoutingKey == "InventoryRejected")
                {
                    var evt = JsonSerializer.Deserialize<InventoryRejected>(body);
                    if (evt != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<ICourseRegistrationRepository>();

                        _logger.LogInformation("InventoryRejected received for {RegistrationId}, performing compensation, CorrelationId={CorrelationId}", evt.RegistrationId, correlationId);
                        // compensate: cancel registration
                        await repo.ChangeStatusCourseAsync(evt.RegistrationId, RegistrationService.Enums.CourseRegistrationStatus.Canceled);

                        // publish OrderCanceled
                        using var connOut = factory.CreateConnection();
                        using var chOut = connOut.CreateModel();
                        var outEvt = new OrderCanceled(evt.RegistrationId, evt.CourseId, DateTime.UtcNow, correlationId);
                        var bytes = JsonSerializer.SerializeToUtf8Bytes(outEvt);
                        var props = chOut.CreateBasicProperties();
                        props.Persistent = true;
                        props.MessageId = Guid.NewGuid().ToString();
                        props.CorrelationId = correlationId;
                        chOut.BasicPublish(exchange, "OrderCanceled", props, bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory result message");
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

    private record InventoryReserved(int RegistrationId, Guid CourseId, DateTime ReservedAt, string CorrelationId);
    private record InventoryRejected(int RegistrationId, Guid CourseId, DateTime RejectedAt, string CorrelationId);
    private record OrderConfirmed(int RegistrationId, Guid CourseId, DateTime ConfirmedAt, string CorrelationId);
    private record OrderCanceled(int RegistrationId, Guid CourseId, DateTime CanceledAt, string CorrelationId);
}
