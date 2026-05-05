namespace OrderService.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
    
    public OrderStatus Status { get; private set; }
    public Order(Guid customerId, Guid productId, int quantity, decimal price)
    {
        CustomerId = customerId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        Status = OrderStatus.Pending;  
    }
    private Order() { }

}