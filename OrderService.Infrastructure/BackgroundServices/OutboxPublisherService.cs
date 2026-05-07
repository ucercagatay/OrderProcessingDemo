using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Hosting;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly string _topic;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(IServiceScopeFactory scopeFactory, IConfiguration configuration,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _topic = configuration["Kafka:Topic"]??"order-events";
        _logger = logger;
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
        };
        _kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var messages = await context.OutboxMessages.Where(x => x.Status == OutboxMessageStatus.Pending)
                    .OrderBy(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync();
                foreach (var message in messages)
                {
                    try
                    {
                        var kafkaMessage = new Message<string, string>()
                        {
                            Key = message.Id.ToString(),
                            Value = message.Payload
                        };
                        await _kafkaProducer.ProduceAsync(_topic, kafkaMessage,stoppingToken);
                        message.MarkAsPublished();
                        _logger.LogInformation("Published outbox message {Id} to Kafka",message.Id);
                    }
                    catch (Exception e)
                    {
                       message.MarkAsFailed(e.Message);
                       _logger.LogInformation(e,"Failed to publish outbox message {Id} to Kafka",message.Id);
                    }
                }

                if (messages.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Error publishing outbox messages");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public override void Dispose()
    {
        _kafkaProducer?.Dispose();
        base.Dispose();
    }
}