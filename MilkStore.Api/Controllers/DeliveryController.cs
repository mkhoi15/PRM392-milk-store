using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto;
using MilkStore.Api.Shared.Dto.Delivery;
using MilkStore.Api.Shared.Enums;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly MilkStoreDbContext _dbContext;

    public DeliveryController(MilkStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpPost]
    [Authorize(Roles = nameof(RoleName.ShopStaff))]
    public async Task<IActionResult> CreateDelivery(DeliveryRequest request)
    {
        var order = await _dbContext.Orders.Include(o => o.User)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId);
        
        if (order == null)
        {
            return NotFound("Order not found.");
        }
        
        if (order.OrderStatus != OrderStatus.Ordered.ToString())
        {
            return BadRequest("The Order have been assigned or delivered.");
        }
        
        var isExistDelivery = await _dbContext.Deliveries.AnyAsync(x => x.OrderId == request.OrderId);
        
        if (isExistDelivery)
        {
            return BadRequest("Delivery has been created.");
        }
        
        var deliveryStaff = await _dbContext.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Id == request.DeliveryStaffId 
                                      && x.Role.Name.Equals(RoleName.DeliveryStaff.ToString()));

        if (deliveryStaff == null)
        {
            return NotFound("Delivery staff not found.");
        }
        
        var delivery = new Delivery()
        {
            OrderId = request.OrderId,
            DeliveryStaffId = request.DeliveryStaffId,
        };
        
        order.OrderStatus = OrderStatus.Assigned.ToString();
        
        _dbContext.Deliveries.Add(delivery);
        
        var result = await _dbContext.SaveChangesAsync();

        if (result > 0)
        {
            // Map to DeliveryResponse DTO
            var deliveryResponse = new DeliveryResponse
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                DeliveryStaffId = delivery.DeliveryStaffId,
                DeliveryDate = delivery.DeliveryDate,
                DeliveryManName = deliveryStaff.FullName,
                Order = new DeliveryOrderResponse
                {
                    CustomerName = order.User?.FullName,
                    OrderCode = order.OrderCode,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    TotalPrice = order.TotalPrice,
                    Address = order.Address,
                    PhoneNumber = order.PhoneNumber
                }
            };
        
            return CreatedAtAction("GetDelivery", new { id = delivery.Id }, deliveryResponse);
        }
       
        return BadRequest();
        
    }
    
    [HttpGet]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.DeliveryStaff)}")]
    public async Task<IActionResult> GetDeliveries(int pageIndex = 1, int pageSize = 10)
    {
        var deliveriesQuery = _dbContext.Deliveries
            .Include(d => d.DeliveryStaff)
            .Include(d => d.Order)
            .ThenInclude(o => o.User)
            .OrderByDescending(d => d.Order.OrderDate);

        deliveriesQuery = deliveriesQuery.OrderByDescending(d => d.Order.OrderDate);

        // Create paginated result from delivery entities
        var pagedDeliveries = await Task.Run(() => 
            PagedResult<Delivery>.CreateAsync(deliveriesQuery, pageIndex, pageSize)
        );

        // Map the paginated result to DeliveryResponse DTOs
        var deliveryResponses = pagedDeliveries.Select(d => new DeliveryResponse
        {
            Id = d.Id,
            OrderId = d.OrderId,
            DeliveryStaffId = d.DeliveryStaffId,
            DeliveryDate = d.DeliveryDate,
            DeliveryManName = d.DeliveryStaff?.FullName,
            Order = new DeliveryOrderResponse
            {
                CustomerName = d.Order?.User?.FullName,
                OrderCode = d.Order?.OrderCode,
                OrderDate = d.Order?.OrderDate,
                OrderStatus = d.Order?.OrderStatus,
                TotalPrice = d.Order?.TotalPrice ?? 0,
                Address = d.Order?.Address,
                PhoneNumber = d.Order?.PhoneNumber
            }
        }).ToList();

        // Create a PagedResult<DeliveryResponse> using the mapped deliveryResponses
        var pagedDeliveryResponses = new PagedResult<DeliveryResponse>(
            deliveryResponses, pagedDeliveries.Count, pageIndex, pageSize
        );

        return Ok(pagedDeliveryResponses);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.DeliveryStaff)}")]
    public async Task<IActionResult> GetDelivery(Guid id)
    {
        var delivery = await _dbContext.Deliveries
                .Include(d => d.DeliveryStaff)
                .Include(d => d.Order)
                .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        
        if (delivery == null)
        {
            return NotFound();
        }
        
        var deliveryResponse = new DeliveryResponse()
        {
            Id = delivery.Id,
            OrderId = delivery.OrderId,
            DeliveryStaffId = delivery.DeliveryStaffId,
            DeliveryDate = delivery.DeliveryDate,
            DeliveryManName = delivery.DeliveryStaff?.FullName,
            Order = new DeliveryOrderResponse()
            {
                CustomerName = delivery.Order?.User?.FullName,
                OrderCode = delivery.Order?.OrderCode,
                OrderDate = delivery.Order?.OrderDate,
                OrderStatus = delivery.Order?.OrderStatus,
                TotalPrice = delivery.Order?.TotalPrice ?? 0,
                Address = delivery.Order?.Address,
                PhoneNumber = delivery.Order?.PhoneNumber
            }
        };
        
        return Ok(deliveryResponse);
    }
    
    [HttpGet("user/{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.DeliveryStaff)}")]
    public async Task<IActionResult> GetDeliveriesByDeliveryStaffId(Guid id, int pageIndex = 1, int pageSize = 10, string? sortBy = "orderDate", string? sortOrder = "desc")
    {
        var deliveriesQuery = _dbContext.Deliveries
            .Include(d => d.DeliveryStaff)
            .Include(d => d.Order)
            .ThenInclude(o => o.User)
            .OrderByDescending(d => d.Order.OrderDate)
            .Where(d => d.DeliveryStaffId == id);
        
        deliveriesQuery = deliveriesQuery.OrderByDescending(d => d.Order.OrderDate);
        
        // Create paginated result from delivery entities
        var pagedDeliveries = await Task.Run(() => 
            PagedResult<Delivery>.CreateAsync(deliveriesQuery, pageIndex, pageSize)
        );
        
        Expression<Func<Delivery, object>> sortByExpression = sortBy switch
        {
            "orderDate" => p => p.Order.OrderDate,
            _ => p => p.Order.OrderDate // Default to sorting by name
        };

        // Apply sorting based on SortOrder
        deliveriesQuery = sortOrder?.ToLower() switch
        {
            "asc" => deliveriesQuery.OrderBy(sortByExpression),
            "desc" => deliveriesQuery.OrderByDescending(sortByExpression),
            _ => deliveriesQuery.OrderBy(sortByExpression)
        };

        // Map the paginated result to DeliveryResponse DTOs
        var deliveryResponses = pagedDeliveries.Select(d => new DeliveryResponse
        {
            Id = d.Id,
            OrderId = d.OrderId,
            DeliveryStaffId = d.DeliveryStaffId,
            DeliveryDate = d.DeliveryDate,
            DeliveryManName = d.DeliveryStaff?.FullName,
            Order = new DeliveryOrderResponse
            {
                CustomerName = d.Order?.User?.FullName,
                OrderCode = d.Order?.OrderCode,
                OrderDate = d.Order?.OrderDate,
                OrderStatus = d.Order?.OrderStatus,
                TotalPrice = d.Order?.TotalPrice ?? 0,
                Address = d.Order?.Address,
                PhoneNumber = d.Order?.PhoneNumber
            }
        }).ToList();

        // Create a PagedResult<DeliveryResponse> using the mapped deliveryResponses
        var pagedDeliveryResponses = new PagedResult<DeliveryResponse>(
            deliveryResponses, pagedDeliveries.Count, pageIndex, pageSize
        );

        return Ok(pagedDeliveryResponses);
    }
    
    [HttpPost("complete/{id}")]
    [Authorize(Roles = nameof(RoleName.DeliveryStaff))]
    public async Task<IActionResult> CompleteDelivery(Guid id)
    {
        var delivery = await _dbContext.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(x => x.Id == id);
        if (delivery == null)
        {
            return NotFound();
        }

        if (delivery.Order!.OrderStatus == OrderStatus.Delivered.ToString())
        {
            return BadRequest("Order has been delivered.");
        }

        if (delivery.Order!.OrderStatus == OrderStatus.Cancelled.ToString())
        {
            return BadRequest("Order has been cancelled.");
        }
        
        delivery.DeliveryDate = DateTime.UtcNow;
        
        delivery.Order.OrderStatus = OrderStatus.Delivered.ToString();
        
        _dbContext.Deliveries.Update(delivery);
        _dbContext.Orders.Update(delivery.Order);
        var result = await _dbContext.SaveChangesAsync();
        
        if (result > 0)
        {
            return Ok();
        }
       
        return BadRequest("Complete delivery failed.");
    }
    
    
}