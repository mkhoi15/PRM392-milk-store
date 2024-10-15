using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/brand")]
public class BrandController : ControllerBase
{
    private readonly MilkStoreDbContext _dbContext;

    public BrandController(MilkStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBrands(string? searchString = null)
    {
        var brandsQuery = _dbContext.Brands.AsQueryable();

        if (searchString != null)
        {
            brandsQuery = brandsQuery.Where(b => b.Name.Contains(searchString));
        }
        
        var brands = await brandsQuery.ToListAsync();
        
        var brandResponses = brands.Select(b => new 
        {
            Id = b.Id,
            Name = b.Name,
        }).ToList();
        

        return Ok(brandResponses);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(Guid id)
    {
        var brand = await _dbContext.Brands.FindAsync(id);

        if (brand == null)
        {
            return NotFound();
        }

        var brandResponse = new 
        {
            Id = brand.Id,
            Name = brand.Name,
        };

        return Ok(brandResponse);
    }
    
}