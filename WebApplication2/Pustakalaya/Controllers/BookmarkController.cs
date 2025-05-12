using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.Models;
using System.Security.Claims;

namespace Pustakalaya.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookmarksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookmarksController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() =>
            User.FindFirstValue(ClaimTypes.Role)?.ToLower() == "admin";

        private bool TryGetMemberId(out long memberId)
        {
            memberId = 0;
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(idStr, out memberId);
        }

        // GET: api/bookmarks
        // List all bookmarks for the current member
        [HttpGet]
        public async Task<IActionResult> GetBookmarks()
        {
            if (!TryGetMemberId(out var memberId))
                return Unauthorized(new { success = false, message = "Invalid user." });

            var bookmarks = await _context.Bookmarks
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Book)
                    .ThenInclude(book => book.Images)
                .Select(b => new
                {
                    BookId   = b.BookId,
                    Title    = b.Book.Title,
                    Author   = b.Book.Author,
                    Price    = b.Book.Price,
                    Images   = b.Book.Images.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Bookmarks retrieved successfully.",
                data    = bookmarks
            });
        }

        // POST: api/bookmarks/{bookId}
        // Add a bookmark for the current member
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddBookmark(long bookId)
        {
            if (IsAdmin())
                return Forbid("Admins cannot bookmark books.");

            if (!TryGetMemberId(out var memberId))
                return Unauthorized(new { success = false, message = "Invalid user." });

            // Check book exists
            if (!await _context.Books.AnyAsync(b => b.Id == bookId))
                return NotFound(new { success = false, message = "Book not found." });

            // Prevent duplicates
            var exists = await _context.Bookmarks
                .AnyAsync(b => b.MemberId == memberId && b.BookId == bookId);

            if (exists)
                return BadRequest(new { success = false, message = "Already bookmarked." });

            var bookmark = new Bookmark
            {
                MemberId = memberId,
                BookId   = bookId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Book added to whitelist." });
        }

        // DELETE: api/bookmarks/{bookId}
        // Remove a bookmark for the current member
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> RemoveBookmark(long bookId)
        {
            if (IsAdmin())
                return Forbid("Admins cannot remove bookmarks.");

            if (!TryGetMemberId(out var memberId))
                return Unauthorized(new { success = false, message = "Invalid user." });

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.MemberId == memberId && b.BookId == bookId);

            if (bookmark == null)
                return NotFound(new { success = false, message = "Bookmark not found." });

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Bookmark removed." });
        }
    }
}