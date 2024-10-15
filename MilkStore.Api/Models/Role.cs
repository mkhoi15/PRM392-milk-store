using System.ComponentModel.DataAnnotations;

namespace MilkStore.Api.Models;

public class Role : IEntity
{
    [Required]
    [StringLength(15, ErrorMessage = "Role name must be less than 15 characters")]
    public string? Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}