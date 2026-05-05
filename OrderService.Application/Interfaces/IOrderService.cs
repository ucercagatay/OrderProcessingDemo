using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken);
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken);
}