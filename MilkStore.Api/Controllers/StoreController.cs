using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/store")]
public class StoreController : ControllerBase
{
    private readonly MilkStoreDbContext _context;

    public StoreController(MilkStoreDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<Store>> GetStores()
    {
        var store = await _context.Stores.FirstOrDefaultAsync();
        if (store == null)
        {
            return NotFound("Don't have any store.");
        }

        return Ok(store);
    }
    
    [HttpPost]
    public async Task<ActionResult<Store>> CreateStore([FromBody] Store store)
    {
        _context.Stores.Add(store);
        await _context.SaveChangesAsync();

        return Ok(store);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStore(Guid id, [FromBody] Store store)
    {
        var existingStore = await _context.Stores.FindAsync(id);
        if (existingStore == null)
        {
            return NotFound("Store not found.");
        }
        
        existingStore.Name = store.Name ?? existingStore.Name;
        existingStore.Address = store.Address ?? existingStore.Address;
        existingStore.PhoneNumber = store.PhoneNumber ?? existingStore.PhoneNumber;
        existingStore.Email = store.Email ?? existingStore.Email;
        
        await _context.SaveChangesAsync();

        return Ok();
    }
    
}