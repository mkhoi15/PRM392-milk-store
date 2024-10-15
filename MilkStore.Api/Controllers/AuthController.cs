using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto.Auth;
using MilkStore.Api.Shared.Enums;
using LoginRequest = MilkStore.Api.Shared.Dto.Auth.LoginRequest;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly MilkStoreDbContext _dbContext;

    public AuthController(IConfiguration configuration, MilkStoreDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid username or password." });
        }

        if (request.Password != user.Password)
        {
            return BadRequest(new { message = "Invalid username or password." });
        }
        
        // Create JWT token
        user.Email ??= string.Empty;
        if (user.Role is null)
        {
            return BadRequest("User don't have role");
        }
        
        DateTime expires = DateTime.Now.AddHours(3);
        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Username)
        };

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtOptions:SecretKey"]));
            
        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken tokenGenerator = new JwtSecurityToken(
            _configuration["JwtOptions:Issuer"],
            _configuration["JwtOptions:Audience"],
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(tokenGenerator);
        
        return Ok(token);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate Role input
        if (!Enum.IsDefined(typeof(RoleName), request.Role))
        {
            return BadRequest(new { message = "Invalid role." });
        }

        // Map integer to RoleName enum
        var roleName = (RoleName)request.Role;

        // Retrieve the Role entity from the database
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName.ToString());

        if (role == null)
        {
            return BadRequest(new { message = "Role not found." });
        }

        // Check if the username or email already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest(new { message = "Username or email already exists." });
        }

        // Create a new User entity
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            RoleId = role.Id,
            Password = request.Password
        };

        // Add the user to the database
        _dbContext.Users.Add(user);
        var result = await _dbContext.SaveChangesAsync();

        if (result > 0)
        {
            return Ok(new { message = "User registered successfully." });
        }
        else
        {
            return BadRequest(new { message = "Failed to register user." });
        }
    }
    
    
}