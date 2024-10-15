using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Order : IEntity
{
    [Required(ErrorMessage = "User ID is required.")]
    public Guid? UserId { get; set; }

    //[Required(ErrorMessage = "Order code is required.")]
    //[Range(1, long.MaxValue, ErrorMessage = "Order code must be a positive number.")]
    public string? OrderCode { get; set; }

    [Required(ErrorMessage = "Order date is required.")]
    public DateTime? OrderDate { get; set; }

    [Required(ErrorMessage = "Order status is required.")]
    [StringLength(50, ErrorMessage = "Order status can't be longer than 50 characters.")]
    public string? OrderStatus { get; set; }

    [Required(ErrorMessage = "Total price is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Total price must be between 0.01 and 1,000,000.")]
    public decimal TotalPrice { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(250, ErrorMessage = "Address can't be longer than 250 characters.")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }
    
    public Delivery? Delivery { get; set; }
    public User? User { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}