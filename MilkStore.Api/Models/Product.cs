using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Product : IEntity
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, ErrorMessage = "Product name can't be longer than 100 characters.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock can't be negative.")]
    public int Stock { get; set; }

    //[Url(ErrorMessage = "Invalid URL format.")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Brand ID is required.")]
    public Guid BrandId { get; set; }
    
    public Brand? Brand { get; set; }
    
    public ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
}