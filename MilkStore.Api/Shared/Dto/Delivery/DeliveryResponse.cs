namespace MilkStore.Api.Shared.Dto.Delivery;

public class DeliveryResponse
{
    public Guid? Id { get; set; }
    
    public Guid OrderId { get; set; }
        
    public Guid DeliveryStaffId { get; set; }
    
    public DateTime? DeliveryDate { get; set; }
    
    public string? DeliveryManName { get; set; }
    
    public DeliveryOrderResponse? Order { get; set; }
    
}

public class DeliveryOrderResponse
{
    public string? CustomerName { get; set; }
    
    public string? OrderCode { get; set; }
    
    public DateTime? OrderDate { get; set; }
    
    public string? OrderStatus { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }
   
}
