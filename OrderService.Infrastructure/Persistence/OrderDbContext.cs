using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext :DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) {}
    public DbSet<Order> Orders { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);                           
                entity.Property(e => e.CustomerId).IsRequired();   
                entity.Property(e => e.ProductId).IsRequired();     
                entity.Property(e => e.Quantity).IsRequired();      
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");  
                entity.Property(e => e.Status)
                    .HasConversion<string>()                     
                    .HasMaxLength(20);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        
    }
}