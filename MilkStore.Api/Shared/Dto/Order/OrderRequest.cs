using System.ComponentModel.DataAnnotations;
using MilkStore.Api.Models;

namespace MilkStore.Api.Shared.Dto.Order;

public class OrderRequest
{
    [Required(ErrorMessage = "User ID is required.")]
    public Guid? UserId { get; set; }
    
    public string? OrderCode { get; set; }

    [Required(ErrorMessage = "Total price is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Total price must be between 0.01 and 1,000,000.")]
    public decimal TotalPrice { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(250, ErrorMessage = "Address can't be longer than 250 characters.")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }
    
    public List<OrderDetailRequest> OrderDetails { get; set; } = new();
}

public class OrderDetailRequest
{
    [Required(ErrorMessage = "Product ID is required.")]
    public Guid? ProductId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1,000.")]
    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
}

public class OrderUpdateRequest
{
    public int? OrderStatus { get; set; }
    
    [StringLength(250, ErrorMessage = "Address can't be longer than 250 characters.")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }
    
    public string? OrderCode { get; set; }
}