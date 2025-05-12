using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.Dtos;

namespace Pustakalaya.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("discounted")]
    public async Task<IActionResult> GetDiscountedBooks()
    {
        var now = DateTime.UtcNow;
        var books = await _context.Books
            .Where(b => b.IsOnSale && b.DiscountStart <= now && 
                        (b.DiscountEnd == null || b.DiscountEnd >= now))
            .Include(b => b.Genre)
            .Include(b => b.Images)
            .OrderByDescending(b => b.DiscountPercentage)
            .Take(12)
            .Select(MapToDto)
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("exclusive")]
    public async Task<IActionResult> GetExclusiveEditions()
    {
        var books = await _context.Books
            .Where(b => b.IsExclusiveEdition)
            .Include(b => b.Genre)
            .Include(b => b.Images)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(MapToDto)
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRatedBooks()
    {
        var books = await _context.Books
            .Where(b => b.AverageRating >= 4.5)
            .Include(b => b.Genre)
            .Include(b => b.Images)
            .OrderByDescending(b => b.AverageRating)
            .ThenByDescending(b => b.TotalSold)
            .Take(10)
            .Select(MapToDto)
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers()
    {
        var books = await _context.Books
            .Include(b => b.Genre)
            .Include(b => b.Images)
            .OrderByDescending(b => b.TotalSold)
            .Take(10)
            .Select(MapToDto)
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("new-arrivals")]
    public async Task<IActionResult> GetNewArrivals()
    {
        var books = await _context.Books
            .Include(b => b.Genre)
            .Include(b => b.Images)
            .OrderByDescending(b => b.PublicationDate)
            .ThenByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(MapToDto)
            .ToListAsync();

        return Ok(books);
    }

    // Mapping logic in LINQ-compatible expression
    private static Expression<Func<Models.Book, BookDto>> MapToDto => b => new BookDto
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        Isbn = b.Isbn,
        Language = b.Language,
        Format = b.Format,
        Price = b.Price,
        Stock = b.Stock,
        GenreName = b.Genre != null ? b.Genre.Name : "",
        PublicationDate = b.PublicationDate,
        IsPhysicalAccess = b.IsPhysicalAccess,
        IsOnSale = b.IsOnSale,
        DiscountPercentage = b.DiscountPercentage,
        DiscountStart = b.DiscountStart,
        DiscountEnd = b.DiscountEnd,
        Description = b.Description,
        Publisher = b.Publisher,
        BookType = b.BookType,
        IsExclusiveEdition = b.IsExclusiveEdition,
        AverageRating = b.AverageRating,
        TotalSold = b.TotalSold,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        Images = b.Images.Select(img => img.Url).ToList()
    };
}
