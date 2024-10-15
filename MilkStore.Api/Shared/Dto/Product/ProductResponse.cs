namespace MilkStore.Api.Shared.Dto.Product;

public class ProductResponse
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }

    public string? Description { get; set; }
    
    public decimal Price { get; set; }

    public int Stock { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public string? BrandName { get; set; }
}