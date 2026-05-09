using System.Text.Json;
using Confluent.Kafka;

namespace NotificationService;

public class NotificationConsumerService:BackgroundService
{
    private readonly ILogger<NotificationConsumerService> _logger;
    private readonly IConsumer<string,string> _consumer;
    private const string Topic = "order-events";
    private const string GroupId ="notification-service-group";

    public NotificationConsumerService(ILogger<NotificationConsumerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var consumerConfig = new ConsumerConfig
        {
            GroupId = GroupId,
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);
        _logger.LogInformation("Notification Service started consuming from {Topic}", Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                if (result?.Message?.Value is null) continue;

                var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(result.Message.Value);

                if (orderEvent is null) continue;
                _logger.LogInformation(
                    "📧 NOTIFICATION: New order received!");
                _logger.LogInformation(
                    "   Sending confirmation email to customer {CustomerId}",
                    orderEvent.CustomerId);
                _logger.LogInformation(
                    "   Order details: ProductId={ProductId}, Qty={Quantity}, Price={Price:C}",
                    orderEvent.ProductId,
                    orderEvent.Quantity,
                    orderEvent.Price);
                _logger.LogInformation(
                    "✅ EMAIL SENT for OrderId={OrderId}", orderEvent.OrderId);
               
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from Kafka");
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _consumer.Close();
        return Task.CompletedTask;
    }

}