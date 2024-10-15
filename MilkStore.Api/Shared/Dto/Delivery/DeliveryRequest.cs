using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Shared.Dto.Delivery;

public class DeliveryRequest
{
    [Required(ErrorMessage = "Order ID is required.")]
    public Guid OrderId { get; set; }
        
    [Required(ErrorMessage = "Delivery Staff ID is required.")]
    public Guid DeliveryStaffId { get; set; }
    
}