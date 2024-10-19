using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        if (searchBy != null)
        {
            usersQuery = searchBy switch
            {
                "username" => usersQuery.Where(u => u.Username.Contains(searchString)),
                "email" => usersQuery.Where(u => u.Email.Contains(searchString)),
                "fullname" => usersQuery.Where(u => u.FullName.Contains(searchString)),
                _ => usersQuery.Where(u => u.FullName.Contains(searchString))
            };
        }
        else
        {
            usersQuery = usersQuery.Where(u => u.FullName.Contains(searchString));
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
    
    
}