using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Delivery : IEntity
{
    [Required(ErrorMessage = "Order ID is required.")]
    public Guid OrderId { get; set; }
        
    [Required(ErrorMessage = "Delivery Staff ID is required.")]
    public Guid DeliveryStaffId { get; set; }
    
    public DateTime? DeliveryDate { get; set; }

    public Order? Order { get; set; }
        
    public User? DeliveryStaff { get; set; }
}