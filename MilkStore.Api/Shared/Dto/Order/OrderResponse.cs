
namespace MilkStore.Api.Shared.Dto.Order;

public class OrderResponse
{
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; }
    
    public string? CustomerName { get; set; }
    
    public string? OrderCode { get; set; }
    
    public DateTime? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }
    
    public List<OrderDetailResponse> OrderDetails { get; set; } = new();
}

public class OrderDetailResponse
{
    public Guid? OrderDetailId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ProductName { get; set; }
}