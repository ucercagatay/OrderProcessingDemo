using System.Text.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class OrderAppService:IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    public OrderAppService(IOrderRepository orderRepository,IOutboxRepository outboxRepository,IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
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
        var payload = JsonSerializer.Serialize(new
        {
            OrderId = order.Id,
            order.CustomerId,
            order.ProductId,
            order.Quantity,
            order.Price
        });
        var outboxMessage = new OutboxMessage("OrderCreated", payload);
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
          await _orderRepository.AddAsync(order, cancellationToken);
          await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);
          await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
        await _orderRepository.AddAsync(order, cancellationToken);
        return OrderMapper.ToDto(order);
        
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(c => OrderMapper.ToDto(c)).ToList();

    }
}