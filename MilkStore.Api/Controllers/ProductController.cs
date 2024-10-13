using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto;
using MilkStore.Api.Shared.Dto.Product;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly MilkStoreDbContext _dbContext;

    public ProductController(MilkStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetProducts(int pageIndex = 1, int pageSize = 10, string? searchString = null, string? searchBy = null)
    {
        
        var productsQuery = _dbContext.Products.AsQueryable();
        
        productsQuery = productsQuery.Include(p => p.Brand);

        if (searchBy != null)
        {
            productsQuery = searchBy switch
            {
                "name" => productsQuery.Where(p => p.Name.Contains(searchString)),
                "description" => productsQuery.Where(p => p.Description.Contains(searchString)),
                "brand" => productsQuery.Where(p => p.Brand.Name.Contains(searchString)),
                _ => productsQuery
            };
        }

        
        var pagedProducts = await Task.Run(() => 
            PagedResult<Product>.CreateAsync(productsQuery, pageIndex, pageSize)
        );

        return Ok(pagedProducts);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("Not found product");
        }

        return Ok(product);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest productRequest)
    {
        Product product = new Product()
        {
            Name = productRequest.Name,
            Description = productRequest.Description,
            Price = productRequest.Price,
            Stock = productRequest.Stock,
            ImageUrl = productRequest.ImageUrl,
            BrandId = productRequest.BrandId
        };
        
        _dbContext.Products.Add(product);
        var result = await _dbContext.SaveChangesAsync();

        if (result > 0)
        {
           return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        
        return BadRequest("Create product failed");
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        var existedProduct = await _dbContext.Products.FindAsync(id);
        if (existedProduct == null)
        {
            return NotFound("Not found product");
        }

        existedProduct.Name = product.Name;
        existedProduct.Description = product.Description;
        existedProduct.Price = product.Price;
        existedProduct.BrandId = product.BrandId;

        await _dbContext.SaveChangesAsync();
        
        return Ok(existedProduct);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var existedProduct = await _dbContext.Products.FindAsync(id);
        if (existedProduct == null)
        {
            return NotFound("Not found product");
        }

        _dbContext.Products.Remove(existedProduct);
        await _dbContext.SaveChangesAsync();
        
        return Ok(existedProduct);
    }
}