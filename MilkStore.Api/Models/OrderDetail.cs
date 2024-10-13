using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class OrderDetail : IEntity
{
    [Required(ErrorMessage = "Order ID is required.")]
    public Guid? OrderId { get; set; }

    [Required(ErrorMessage = "Product ID is required.")]
    public Guid? ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000.")]
    public decimal Price { get; set; }
    
    public Product? Product { get; set; }
    public Order? Order { get; set; }
    
}