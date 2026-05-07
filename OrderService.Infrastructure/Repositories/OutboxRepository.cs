using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly OrderDbContext _dbContext;
    public OutboxRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken ct)
    {
       await  _dbContext.OutboxMessages.AddAsync(outboxMessage, ct);
    }

    public async Task UpdateAsync(OutboxMessage outboxMessage, CancellationToken ct)
    {
      _dbContext.OutboxMessages.Update(outboxMessage);
      await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct)
    {
        return await _dbContext.OutboxMessages.Where(x => x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}