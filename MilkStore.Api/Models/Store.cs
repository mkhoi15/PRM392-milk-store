using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Store : IEntity
{
    [Required(ErrorMessage = "Store name is required.")]
    [StringLength(100, ErrorMessage = "Store name can't be longer than 100 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(250, ErrorMessage = "Address can't be longer than 250 characters.")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }
}