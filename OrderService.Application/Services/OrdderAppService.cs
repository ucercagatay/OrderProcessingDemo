using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;

namespace OrderService.Application.Services;

public class OrdderAppService:IOrderService
{
    private readonly IOrderRepository _orderRepository;
    public OrdderAppService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null) return null;
        var orderDto = OrderMapper.ToDto(order);
        return orderDto;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var order = OrderMapper.ToEntity(dto);
        await _orderRepository.AddAsync(order, cancellationToken);
        return OrderMapper.ToDto(order);
        
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(c => OrderMapper.ToDto(c)).ToList();

    }
}