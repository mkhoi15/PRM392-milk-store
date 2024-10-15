using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Shared.Dto.User;

public class UpdateUserRequest
{
    [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters.")]
    public string? Username { get; set; }
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string? Password { get; set; }
    
    [StringLength(100, ErrorMessage = "Full name can't be longer than 100 characters.")]
    public string? FullName { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number can't be longer than 15 digits.")]
    public string? PhoneNumber { get; set; }
}