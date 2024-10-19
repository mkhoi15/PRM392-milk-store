using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkStore.Api.Models;
using MilkStore.Api.Shared.Dto;
using MilkStore.Api.Shared.Dto.Order;
using MilkStore.Api.Shared.Enums;

namespace MilkStore.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly MilkStoreDbContext _context;

    public OrderController(MilkStoreDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}")]
    public async Task<ActionResult> GetOrders(int pageIndex = 1, int pageSize = 10, string? searchString = null, string? searchBy = null)
    {
        var ordersQuery = _context.Orders.AsQueryable();
        
        if (searchBy != null)
        {
            ordersQuery = searchBy switch
            {
                "orderStatus" => ordersQuery.Where(o => o.OrderStatus.Contains(searchString)),
                "address" => ordersQuery.Where(o => o.Address.Contains(searchString)),
                "phoneNumber" => ordersQuery.Where(o => o.PhoneNumber.Contains(searchString)),
                _ => ordersQuery.Where(o => o.Address.Contains(searchString)),
            };
        }
        else
        {
            ordersQuery = ordersQuery.Where(o => o.Address.Contains(searchString));
        }
        
        var pagedOrders = await Task.Run(() => 
            PagedResult<Order>.CreateAsync(ordersQuery, pageIndex, pageSize)
        );
        
        var orderResponses = pagedOrders.Select(o => new OrdersResponse()
        {
            Id = o.Id,
            UserId = o.UserId,
            OrderCode = o.OrderCode,
            OrderDate = o.OrderDate,
            OrderStatus = o.OrderStatus,
            TotalPrice = o.TotalPrice,
            Address = o.Address,
            PhoneNumber = o.PhoneNumber
        }).ToList();
        
        
        var pagedOrderResponses = new PagedResult<OrdersResponse>(
            orderResponses, pagedOrders.Count, pageIndex, pageSize
        );
        
        return Ok(pagedOrderResponses);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}")]
    public async Task<ActionResult> GetOrder(Guid id)
    {
        var orderQuery = _context.Orders.AsQueryable();
        
        var order = await orderQuery
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        
        if (order == null)
        {
            return NotFound();
        }
        
        var user = await _context.Users.FindAsync(order.UserId);
        
        var orderResponse = new OrderResponse()
        {
            Id = order.Id,
            UserId = order.UserId,
            CustomerName = user!.FullName,
            OrderCode = order.OrderCode,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalPrice = order.TotalPrice,
            Address = order.Address,
            PhoneNumber = order.PhoneNumber,
            OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse()
            {
                OrderDetailId = od.Id,
                ProductName = od.Product!.Name,
                Quantity = od.Quantity,
                Price = od.Price
                
            }).ToList()
        };
        
        return Ok(orderResponse);
    }
    
    [HttpGet("user/{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}")]
    public async Task<ActionResult> GetOrdersByUserId(Guid id, int pageIndex = 1, int pageSize = 10, string? searchString = null, string? searchBy = null)
    {
        var ordersQuery = _context.Orders.AsQueryable();
        
        if (searchBy != null)
        {
            ordersQuery = searchBy switch
            {
                "orderStatus" => ordersQuery.Where(o => o.OrderStatus.Contains(searchString)),
                "address" => ordersQuery.Where(o => o.Address.Contains(searchString)),
                "phoneNumber" => ordersQuery.Where(o => o.PhoneNumber.Contains(searchString)),
                _ => ordersQuery
            };
        }
        
        ordersQuery = ordersQuery.Where(o => o.UserId == id);
        
        var pagedOrders = await Task.Run(() => 
            PagedResult<Order>.CreateAsync(ordersQuery, pageIndex, pageSize)
        );
        
        var orderResponses = pagedOrders.Select(o => new OrdersResponse()
        {
            Id = o.Id,
            UserId = o.UserId,
            OrderCode = o.OrderCode,
            OrderDate = o.OrderDate,
            OrderStatus = o.OrderStatus,
            TotalPrice = o.TotalPrice,
            Address = o.Address,
            PhoneNumber = o.PhoneNumber
        }).ToList();
        
        
        var pagedOrderResponses = new PagedResult<OrdersResponse>(
            orderResponses, pagedOrders.Count, pageIndex, pageSize
        );
        
        return Ok(pagedOrderResponses);
    }
    
    [HttpPost]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}")]
    public async Task<ActionResult> CreateOrder(OrderRequest orderRequest)
    {
        // Start a transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Check if all products in the order have enough stock
            foreach (var orderDetailRequest in orderRequest.OrderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetailRequest.ProductId);

                if (product == null)
                {
                    return NotFound($"Product with ID {orderDetailRequest.ProductId} not found.");
                }

                if (product.Stock < orderDetailRequest.Quantity)
                {
                    return BadRequest($"Not enough stock for product {product.Name}. Available stock: {product.Stock}.");
                }
            }

            // Create Order
            var order = new Order
            {
                UserId = orderRequest.UserId,
                OrderDate = DateTime.UtcNow.AddHours(7),
                OrderStatus = OrderStatus.Ordered.ToString(), 
                TotalPrice = orderRequest.TotalPrice,
                Address = orderRequest.Address,
                PhoneNumber = orderRequest.PhoneNumber
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create OrderDetails and update product stock
            foreach (var orderDetailRequest in orderRequest.OrderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetailRequest.ProductId);

                // Create OrderDetail
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = orderDetailRequest.ProductId,
                    Quantity = orderDetailRequest.Quantity,
                    Price = product!.Price * orderDetailRequest.Quantity
                };

                _context.OrderDetails.Add(orderDetail);

                // Reduce product stock
                product.Stock -= orderDetailRequest.Quantity;
                _context.Products.Update(product);
            }

            // Save changes (both OrderDetails and updated Product stock)
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return Created();
        }
        catch (Exception ex)
        {
            // Rollback transaction if any error occurs
            await transaction.RollbackAsync();

            // Return an internal server error
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}, {nameof(RoleName.DeliveryStaff)}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderUpdateRequest order)
    {
        var existedOrder = await _context.Orders.FindAsync(id);
        if (existedOrder == null)
        {
            return NotFound("Not found order");
        }
        
        if (order.OrderStatus is not null && Enum.IsDefined(typeof(OrderStatus), order.OrderStatus))
        {
            var orderStatus = (OrderStatus)order.OrderStatus;
            existedOrder.OrderStatus = orderStatus.ToString();
        }
        
        existedOrder.Address = order.Address ?? existedOrder.Address;
        existedOrder.PhoneNumber = order.PhoneNumber ?? existedOrder.PhoneNumber;
        existedOrder.OrderCode = order.OrderCode ?? existedOrder.OrderCode;
        
        
        return Ok(existedOrder);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{nameof(RoleName.ShopStaff)},{nameof(RoleName.Customer)}")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var existedOrder = await _context.Orders
            .Include(o => o.OrderDetails) // Include OrderDetails
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existedOrder == null)
        {
            return NotFound("Order not found");
        }

        if (existedOrder.OrderStatus == OrderStatus.Ordered.ToString())
        {
            // Start a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update the stock of each product in the order
                foreach (var orderDetail in existedOrder.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(orderDetail.ProductId); // Explicitly fetch the product by ProductId

                    if (product != null)
                    {
                        product.Stock += orderDetail.Quantity; // Increase the stock by the ordered quantity
                        _context.Products.Update(product);
                    }
                }

                // Set order status to be Cancelled
                existedOrder.OrderStatus = OrderStatus.Cancelled.ToString();
                _context.Orders.Update(existedOrder);

                // Save changes to update both the Order and Product stock
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                return Ok("Order canceled and stock updated");
            }
            catch (Exception ex)
            {
                // Rollback the transaction if an error occurs
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        return BadRequest("Cannot cancel this order");
    }

    
}