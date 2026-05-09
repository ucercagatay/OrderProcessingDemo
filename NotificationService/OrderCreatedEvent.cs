namespace NotificationService;

public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, Guid ProductId, int Quantity, decimal Price);