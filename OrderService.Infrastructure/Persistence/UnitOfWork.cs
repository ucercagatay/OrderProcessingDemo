using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(OrderDbContext context)
    {
        _context = context;
    }
    public async Task BeginTransactionAsync(CancellationToken ct)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
        => await _transaction!.CommitAsync(ct);

    public async Task RollbackAsync(CancellationToken ct)
        => await _transaction!.RollbackAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct)
        => await _context.SaveChangesAsync(ct);
}