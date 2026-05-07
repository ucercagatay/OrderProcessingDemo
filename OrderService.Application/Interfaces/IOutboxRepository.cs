using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage outboxMessage,CancellationToken ct);
    Task UpdateAsync(OutboxMessage outboxMessage,CancellationToken ct);
    Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize,CancellationToken ct);
}