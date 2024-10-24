namespace MilkStore.Api.Shared.Dto.Order;

public class OrdersResponse
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
}