using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.DTOs;
using Pustakalaya.Models;
using System.Security.Claims;

namespace Pustakalaya.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "member")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    private long GetMemberId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ReviewCreateDto dto)
    {
        var memberId = GetMemberId();

        var book = await _context.Books.FindAsync(dto.BookId);
        if (book == null) return NotFound(new { message = "Book not found." });

        var alreadyExists = await _context.Reviews
            .AnyAsync(r => r.MemberId == memberId && r.BookId == dto.BookId);
        if (alreadyExists)
            return Conflict(new { message = "You have already reviewed this book." });

        var review = new Review
        {
            MemberId = memberId,
            BookId = dto.BookId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await UpdateBookAverageRating(dto.BookId);

        return Ok(new { message = "Review submitted successfully." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(long id, [FromBody] ReviewUpdateDto dto)
    {
        var memberId = GetMemberId();

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.MemberId == memberId);

        if (review == null)
            return NotFound(new { message = "Review not found or not yours." });

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await UpdateBookAverageRating(review.BookId);

        return Ok(new { message = "Review updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(long id)
    {
        var memberId = GetMemberId();

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.MemberId == memberId);
        if (review == null)
            return NotFound(new { message = "Review not found or not yours." });

        var bookId = review.BookId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        await UpdateBookAverageRating(bookId);

        return Ok(new { message = "Review deleted." });
    }

    [HttpGet("book/{bookId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewsForBook(long bookId)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists) return NotFound(new { message = "Book not found." });

        var reviews = await _context.Reviews
            .Where(r => r.BookId == bookId)
            .Include(r => r.Member)
            .Include(r => r.Book)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = reviews.Select(BuildDtoFromEntity).ToList();
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var memberId = GetMemberId();

        var reviews = await _context.Reviews
            .Where(r => r.MemberId == memberId)
            .Include(r => r.Book)
            .Include(r => r.Member)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = reviews.Select(BuildDtoFromEntity).ToList();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewById(long id)
    {
        var review = await _context.Reviews
            .Include(r => r.Book)
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return NotFound(new { message = "Review not found." });

        return Ok(BuildDtoFromEntity(review));
    }

    private ReviewResponseDto BuildDtoFromEntity(Review r)
    {
        return new ReviewResponseDto
        {
            Id = r.Id,
            MemberId = r.MemberId,
            MemberName = r.Member?.Name ?? "Unknown",
            BookId = r.BookId,
            BookTitle = r.Book?.Title ?? "Unknown",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        };
    }

    private async Task UpdateBookAverageRating(long bookId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.BookId == bookId)
            .ToListAsync();

        var book = await _context.Books.FindAsync(bookId);
        if (book == null) return;

        book.AverageRating = reviews.Count > 0
            ? Math.Round(reviews.Average(r => r.Rating), 2)
            : 0;

        await _context.SaveChangesAsync();
    }
}
