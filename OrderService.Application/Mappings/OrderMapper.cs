using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings;

public static class OrderMapper
{
    public static OrderDto ToDto(Order order)
    {
        var dto = new OrderDto();
        dto.Id = order.Id;
        dto.CustomerId = order.CustomerId;
        dto.Quantity = order.Quantity;
        dto.Status = order.Status.ToString();
        dto.Price = order.Price;
        dto.CreatedAt = order.CreatedAt;
        dto.ProductId = order.ProductId;
        return dto;
    }

    public static Order ToEntity(CreateOrderDto dto)
    {
        var order = new Order(dto.CustomerId, dto.ProductId, dto.Quantity, dto.Price);
        
        return order;
    }
}