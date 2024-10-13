using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MilkStore.Api.Models;

public class IEntity
{
    [Key]
    public Guid? Id { get; set; }
}