using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto;
using MilkStore.Api.Shared.Dto.Product;
using MilkStore.Api.Shared.Enums;

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
        var productsQuery = _dbContext.Products
            .Include(p => p.Brand)
            .Where(p => p.IsDeleted == false);

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

        // Create paginated result from product entities
        var pagedProducts = await Task.Run(() => 
            PagedResult<Product>.CreateAsync(productsQuery, pageIndex, pageSize)
        );

        // Map the paginated result to ProductResponse DTOs
        var productResponses = pagedProducts.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            IsDeleted = p.IsDeleted,
            BrandName = p.Brand?.Name
        }).ToList();

        // Create a PagedResult<ProductResponse> using the mapped productResponses
        var pagedProductResponses = new PagedResult<ProductResponse>(
            productResponses, pagedProducts.Count, pageIndex, pageSize
        );

        return Ok(pagedProductResponses);
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _dbContext.Products
            .Include(p => p.Brand)  // Include the Brand so we can map BrandName
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
    
        if (product == null)
        {
            return NotFound("Not found product");
        }

        var productResponse = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            IsDeleted = product.IsDeleted,
            BrandName = product.Brand?.Name  // Map the BrandName if Brand exists
        };

        return Ok(productResponse);
    }

    
    [HttpPost]
    [Authorize(Roles = nameof(RoleName.Admin))]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest productRequest)
    {
        Product product = new Product()
        {
            Name = productRequest.Name,
            Description = productRequest.Description,
            Price = productRequest.Price,
            Stock = productRequest.Stock,
            ImageUrl = productRequest.ImageUrl,
            BrandId = productRequest.BrandId,
            IsDeleted = false
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
    [Authorize(Roles = nameof(RoleName.Admin))]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateRequest product)
    {
        var existedProduct = await _dbContext.Products.FindAsync(id);
        if (existedProduct == null || existedProduct.IsDeleted)
        {
            return NotFound("Not found product");
        }

        existedProduct.Name = product.Name ?? existedProduct.Name;
        existedProduct.Description = product.Description ?? existedProduct.Description;
        existedProduct.Price = product.Price ?? existedProduct.Price;
        existedProduct.Stock = product.Stock ?? existedProduct.Stock;
        existedProduct.ImageUrl = product.ImageUrl ?? existedProduct.ImageUrl;

        await _dbContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleName.Admin))]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var existedProduct = await _dbContext.Products.FindAsync(id);
        if (existedProduct == null || existedProduct.IsDeleted)
        {
            return NotFound("Not found product");
        }
        
        existedProduct.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();
        
        return Ok();
    }
}