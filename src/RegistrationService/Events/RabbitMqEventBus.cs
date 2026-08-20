using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RegistrationService.Events
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly IConfiguration _config;
        private readonly ConnectionFactory _factory;

        public RabbitMqEventBus(ILogger<RabbitMqEventBus> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"] ?? "localhost"
            };
        }

        public Task PublishAsync(object @event)
        {
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel = connection.CreateModel();

                var exchange = _config["RabbitMQ:Exchange"] ?? "learning";
                channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true, autoDelete: false);

                var routingKey = @event.GetType().Name; // use type name as routing key
                var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());

                var props = channel.CreateBasicProperties();
                props.Persistent = true;
                props.MessageId = Guid.NewGuid().ToString();
                // if event has CorrelationId property, use it to propagate; otherwise set a new one
                var corrProp = @event.GetType().GetProperty("CorrelationId");
                if (corrProp != null)
                {
                    var val = corrProp.GetValue(@event)?.ToString();
                    props.CorrelationId = !string.IsNullOrEmpty(val) ? val : Guid.NewGuid().ToString();
                }
                else
                {
                    props.CorrelationId = Guid.NewGuid().ToString();
                }

                channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: props, body: body);

                _logger.LogInformation("Published event {Event} MessageId={MessageId} CorrelationId={CorrelationId}", routingKey, props.MessageId, props.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed publishing event {@Event}", @event);
            }

            return Task.CompletedTask;
        }
    }
}
