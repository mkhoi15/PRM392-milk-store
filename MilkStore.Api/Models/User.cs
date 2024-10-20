using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class User : IEntity
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string? Password { get; set; }

    [StringLength(100, ErrorMessage = "Full name can't be longer than 100 characters.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Role ID is required.")]
    public Guid? RoleId { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public Role? Role { get; set; }
    
    public virtual ICollection<Delivery>? Deliveries { get; set; } = new List<Delivery>();
    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
    
}