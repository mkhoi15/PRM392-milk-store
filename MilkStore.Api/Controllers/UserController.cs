using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto;
using MilkStore.Api.Shared.Dto.User;
using MilkStore.Api.Shared.Enums;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly MilkStoreDbContext _dbContext;

    public UserController(MilkStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Admin)}")]
    public async Task<IActionResult> GetUsers(int pageIndex = 1, int pageSize = 10, string? searchString = null, string? searchBy = null)
    {
        
        var usersQuery = _dbContext.Users.AsQueryable();

        Expression<Func<User, bool>> searchByExpression = searchBy switch
        {
            "username" => u => u.Username.Contains(searchString!),
            "email" => u => u.Email.Contains(searchString!),
            "fullname" => u => u.FullName.Contains(searchString!),
            _ => u => u.FullName.Contains(searchString!) // Default to search by FullName
        };

        // Apply search filter if searchString is not empty
        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(searchByExpression);
        }

        usersQuery = usersQuery.Where(u => u.IsDeleted == false);
        
        var pagedUsers = await Task.Run(() => 
            PagedResult<User>.CreateAsync(usersQuery, pageIndex, pageSize)
        );

        return Ok(pagedUsers);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound("Not found user");
        }

        return Ok(user);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest user)
    {
        var existedUser = await _dbContext.Users.FindAsync(id);
        if (existedUser == null || existedUser.IsDeleted)
        {
            return NotFound("Not found user");
        }

        existedUser.Username = user.Username ?? existedUser.Username;
        existedUser.Email = user.Email ?? existedUser.Email;
        existedUser.FullName = user.FullName ?? existedUser.FullName;
        existedUser.PhoneNumber = user.PhoneNumber ?? existedUser.PhoneNumber;
        existedUser.Password = user.Password ?? existedUser.Password;

        _dbContext.Users.Update(existedUser);
        
        await _dbContext.SaveChangesAsync();

        return Ok(existedUser);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var existedUser = await _dbContext.Users.FindAsync(id);
        if (existedUser == null || existedUser.IsDeleted)
        {
            return NotFound("Not found user");
        }

        existedUser.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();

        return Ok("Deleted user");
    }
    
    [HttpGet("/delivery-man")]
    public async Task<IActionResult> GetDelivery()
    {
        
        var usersQuery = _dbContext.Users
            .AsQueryable();

        usersQuery = usersQuery.Where(u => u.Role.Name == RoleName.DeliveryStaff.ToString() && u.IsDeleted == false);

        var delivery = await usersQuery.Select(
            u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                u.PhoneNumber
            }
        ).ToListAsync();
        
        return Ok(delivery);
    } 
}