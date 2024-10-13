using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Shared.Dto.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
}