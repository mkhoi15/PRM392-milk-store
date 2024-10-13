using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Brand : IEntity
{
    [StringLength(50, ErrorMessage = "Brand name can't be longer than 50 characters.")]
    public string? Name { get; set; }
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}