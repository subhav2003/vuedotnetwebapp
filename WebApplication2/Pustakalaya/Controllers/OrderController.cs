using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.DTOs;
using Pustakalaya.Models;
using System.Security.Cryptography;

namespace Pustakalaya.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    private long? GetUserId()
    {
        var idStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(idStr, out var id) ? id : null;
    }

    private string GenerateClaimCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3];
        rng.GetBytes(bytes);
        int code = BitConverter.ToInt32(bytes.Concat(new byte[1]).ToArray(), 0) % 1000000;
        return code.ToString("D6");
    }

    [HttpPost]
    [Authorize(Roles = "member")]
    public async Task<ActionResult> CreateOrder()
    {
        var memberId = GetUserId();
        if (memberId == null) return Unauthorized(new { success = false, message = "Invalid member ID." });

        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(c => c.MemberId == memberId);

        if (cart == null || cart.Items.Count == 0)
            return BadRequest(new { success = false, message = "Your cart is empty." });

        // Validate stock
        foreach (var item in cart.Items)
        {
            if (item.Book == null)
                return BadRequest(new { success = false, message = $"Book with ID {item.BookId} not found." });

            if (item.Quantity > item.Book.Stock)
                return BadRequest(new
                {
                    success = false,
                    message = $"Not enough stock for '{item.Book.Title}'. Available: {item.Book.Stock}, Requested: {item.Quantity}"
                });
        }

        var order = new Order
        {
            MemberId = memberId.Value,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ClaimCode = GenerateClaimCode(),
            FulfillmentMethod = "pickup",
            OrderStatus = "pending"
        };

        var items = new List<OrderItem>();
        decimal total = 0;

        foreach (var cartItem in cart.Items)
        {
            var book = cartItem.Book!;
            var unitPrice = book.Price;
            var lineTotal = unitPrice * cartItem.Quantity;

            items.Add(new OrderItem
            {
                BookId = book.Id,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                DiscountApplied = 0,
                LineTotal = lineTotal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Deduct stock and increase total sold
            book.Stock -= cartItem.Quantity;
            book.TotalSold += cartItem.Quantity;

            total += lineTotal;
        }

        // Apply discounts
        var discounts = new List<string>();
        if (items.Sum(i => i.Quantity) >= 5)
        {
            var bulkDiscount = total * 0.05m;
            total -= bulkDiscount;
            order.DiscountAmount += bulkDiscount;
            discounts.Add("bulk");
        }

        var completedOrders = await _context.Orders
            .CountAsync(o => o.MemberId == memberId && o.OrderStatus == "completed");

        if ((completedOrders + 1) % 11 == 0)
        {
            var loyaltyDiscount = total * 0.10m;
            total -= loyaltyDiscount;
            order.DiscountAmount += loyaltyDiscount;
            discounts.Add("loyalty");
        }

        order.TotalPrice = total;
        order.AppliedDiscounts = string.Join(",", discounts);
        order.PickupDeadline = order.OrderDate.AddDays(7);
        order.OrderItems = items;

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Order placed successfully.",
            data = await BuildOrderResponse(order.Id)
        });
    }


    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                    .ThenInclude(b => b.Images)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(BuildOrderResponseFromEntity).ToList();
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "member")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetMyOrders()
    {
        var memberId = GetUserId();
        if (memberId == null) return Unauthorized("Invalid user ID.");

        var orders = await _context.Orders
            .Where(o => o.MemberId == memberId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                    .ThenInclude(b => b.Images)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(BuildOrderResponseFromEntity).ToList();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderResponseDto>> GetOrderById(long id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound("Order not found.");
        return Ok(BuildOrderResponseFromEntity(order));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(long id, [FromBody] OrderStatusUpdateDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound("Order not found.");

        order.OrderStatus = dto.OrderStatus;
        if (dto.IsPaid.HasValue)
            order.IsPaid = dto.IsPaid.Value;

        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(BuildOrderResponseFromEntity(order));
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "member")]
    public async Task<ActionResult<OrderResponseDto>> CancelOrder(long id)
    {
        var memberId = GetUserId();
        if (memberId == null) return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.Id == id && o.MemberId == memberId);

        if (order == null)
            return NotFound("Order not found or not yours.");

        if (order.OrderStatus != "pending")
            return BadRequest("Only pending orders can be cancelled.");

        order.OrderStatus = "cancelled";
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(BuildOrderResponseFromEntity(order));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<OrderResponseDto>> DeleteOrder(long id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound("Order not found.");

        var dto = BuildOrderResponseFromEntity(order);

        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(dto);
    }

    // Helper: Convert Order entity to DTO
    private OrderResponseDto BuildOrderResponseFromEntity(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            ClaimCode = order.ClaimCode,
            TotalPrice = order.TotalPrice,
            DiscountAmount = order.DiscountAmount,
            AppliedDiscounts = order.AppliedDiscounts,
            OrderStatus = order.OrderStatus,
            IsPaid = order.IsPaid,
            OrderDate = order.OrderDate,
            PickupDeadline = order.PickupDeadline,
            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                BookId = i.BookId,
                Title = i.Book?.Title ?? "Unknown",
                Image = i.Book?.Images.FirstOrDefault()?.Url ?? "/fallback.png",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountApplied = i.DiscountApplied
            }).ToList()
        };
    }
    
    [HttpPut("claim/{code}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<OrderResponseDto>> ClaimOrderByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Claim code is required.");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Book)
            .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.ClaimCode == code);

        if (order == null)
            return NotFound("No order found with the provided claim code.");

        if (order.OrderStatus != "pending")
            return BadRequest("Only pending orders can be claimed.");

        order.OrderStatus = "claimed";
        order.IsPaid = true;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(BuildOrderResponseFromEntity(order));
    }

    
    private async Task<OrderResponseDto> BuildOrderResponse(long id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Book)
            .ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new Exception("Order not found.");

        return BuildOrderResponseFromEntity(order);
    }

}
