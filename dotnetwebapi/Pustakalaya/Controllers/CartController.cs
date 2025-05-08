using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.Dtos;
using Pustakalaya.Models;
using System.Security.Claims;

namespace Pustakalaya.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private long GetMemberId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var memberId = GetMemberId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .ThenInclude(b => b.Images)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            var products = cart?.Items.Select(i => new CartItemDto
            {
                BookId = i.Book.Id,
                Title = i.Book.Title,
                Price = i.Book.Price,
                Quantity = i.Quantity,
                Images = i.Book.Images.Select(img => img.Url).ToList()
            }).ToList() ?? new List<CartItemDto>();

            return Ok(new { cart = new { products } });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var memberId = GetMemberId();
            var book = await _context.Books.FindAsync(request.ProductId);
            if (book == null) return NotFound(new { message = "Book not found." });

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
            {
                cart = new Cart
                {
                    MemberId = memberId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var item = cart.Items.FirstOrDefault(i => i.BookId == request.ProductId);
            if (item != null)
            {
                item.Quantity += request.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    BookId = request.ProductId,
                    Quantity = request.Quantity,
                    DateAdded = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item added to cart." });
        }

        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateQuantity(long productId, [FromBody] UpdateCartRequest request)
        {
            var memberId = GetMemberId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.BookId == productId && i.Cart.MemberId == memberId);

            if (item == null) return NotFound(new { message = "Item not found in cart." });

            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Quantity updated." });
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(long productId)
        {
            var memberId = GetMemberId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.BookId == productId && i.Cart.MemberId == memberId);

            if (item == null) return NotFound(new { message = "Item not found in cart." });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from cart." });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var memberId = GetMemberId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null || !cart.Items.Any())
                return Ok(new { message = "Cart is already empty." });

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart cleared." });
        }
    }

    public class AddToCartRequest
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartRequest
    {
        public int Quantity { get; set; }
    }
}
