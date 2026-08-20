using System.Text;
using System.Text.Json;
using NotificationService.DTOs;
using NotificationService.Interfaces.IServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Consumers;

public class RegistrationCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegistrationCreatedConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;

    public RegistrationCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<RegistrationCreatedConsumer> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // Retry connecting to RabbitMQ up to 10 times with delay
    IConnection? connection = null;
    var factory = new ConnectionFactory
    {
        HostName = _config["RabbitMQ:Host"] ?? "localhost"
    };

    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            connection = factory.CreateConnection();
            _logger.LogInformation("Connected to RabbitMQ on attempt {Attempt}", attempt);
            break;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("RabbitMQ connection attempt {Attempt}/10 failed: {Message}", attempt, ex.Message);
            if (attempt == 10) throw;
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    _connection = connection;
    _channel = _connection!.CreateModel();
    _channel.QueueDeclare("registration.created", durable: true, exclusive: false, autoDelete: false);

    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += async (_, ea) =>
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var evt = JsonSerializer.Deserialize<RegistrationCreatedEvent>(json);
        if (evt == null) return;

        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();

        await service.SendNotificationAsync(new CreateNotificationDto
        {
            UserId = evt.UserId,
            CourseId = evt.CourseId,
            Type = "Confirmation",
            Message = $"Your registration for course {evt.CourseId} has been confirmed."
        });

        _logger.LogInformation("Processed RegistrationCreated event for user {UserId}", evt.UserId);
        _channel.BasicAck(ea.DeliveryTag, false);
    };

    _channel.BasicConsume("registration.created", autoAck: false, consumer: consumer);
}

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
