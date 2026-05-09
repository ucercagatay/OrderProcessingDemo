using System.Text.Json;
using Confluent.Kafka;

namespace InventoryService;

public class InventoryConsumerService:BackgroundService
{
    private readonly ILogger<InventoryConsumerService> _logger;
    private readonly IConsumer<string,string> _consumer;
    private const string Topic = "order-events";
    private const string GroupId = "inventory-service-group";

    public InventoryConsumerService(ILogger<InventoryConsumerService> logger, IConfiguration configuration)
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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            _consumer.Subscribe(Topic);
            _logger.LogInformation("Inventory Service started consuming from {Topic}", Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result?.Message?.Value is null) continue;

                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(result.Message.Value);

                    if (orderEvent is null) continue;

                    _logger.LogInformation(
                        "📦 INVENTORY CHECK: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                        orderEvent.OrderId,
                        orderEvent.ProductId,
                        orderEvent.Quantity);

                    if (orderEvent.Quantity > 100)
                    {
                        _logger.LogWarning(
                            "⚠️ INSUFFICIENT STOCK: OrderId={OrderId}, Requested={Quantity}",
                            orderEvent.OrderId,
                            orderEvent.Quantity);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "✅ STOCK RESERVED: OrderId={OrderId}, Quantity={Quantity}",
                            orderEvent.OrderId,
                            orderEvent.Quantity);
                    }
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
        }, stoppingToken);
    }
    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}