using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.Models;
using Pustakalaya.Dtos;
using System.Security.Claims;
using Pustakalaya.DTOs;

namespace Pustakalaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => User.FindFirstValue(ClaimTypes.Role) == "admin";
        private DateTime ToUtc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        private DateTime? ToUtc(DateTime? dt) => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

        // ========== BOOK ==========

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Images)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Isbn = b.Isbn,
                    Language = b.Language,
                    Format = b.Format,
                    Price = b.Price,
                    Stock = b.Stock,
                    GenreName = b.Genre.Name,
                    PublicationDate = b.PublicationDate,
                    IsPhysicalAccess = b.IsPhysicalAccess,
                    IsOnSale = b.IsOnSale,
                    DiscountPercentage = b.DiscountPercentage,
                    DiscountStart = b.DiscountStart,
                    DiscountEnd = b.DiscountEnd,
                    Images = b.Images.Select(img => img.Url).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, message = "Books retrieved successfully.", data = books });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBook(long id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Images)
                .Where(b => b.Id == id)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Isbn = b.Isbn,
                    Language = b.Language,
                    Format = b.Format,
                    Price = b.Price,
                    Stock = b.Stock,
                    GenreName = b.Genre.Name,
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
                })

                .FirstOrDefaultAsync();

            if (book == null)
                return NotFound(new { success = false, message = "Book not found." });

            return Ok(new { success = true, message = "Book retrieved successfully.", data = book });
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> PostBook([FromForm] BookCreateDto dto, [FromForm] List<IFormFile>? images)
        {
            if (!IsAdmin()) return Forbid("Only admin users can add books.");

            if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
                return Unauthorized(new { success = false, message = "Invalid user ID in token." });

            if (!_context.Genres.Any(g => g.Id == dto.GenreId))
                return BadRequest(new { success = false, message = "Invalid Genre ID." });

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                GenreId = dto.GenreId,
                Language = dto.Language,
                Format = dto.Format,
                Price = dto.Price,
                IsPhysicalAccess = dto.IsPhysicalAccess,
                PublicationDate = ToUtc(dto.PublicationDate),
                Stock = dto.Stock,
                Isbn = dto.Isbn,
                Description = dto.Description,
                Publisher = dto.Publisher,
                BookType = dto.BookType,
                IsExclusiveEdition = dto.IsExclusiveEdition,
                IsOnSale = dto.IsOnSale,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountStart = ToUtc(dto.DiscountStart),
                DiscountEnd = ToUtc(dto.DiscountEnd),
                AdminId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Images = new List<BookImage>()
            };


            if (images != null)
            {
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine("wwwroot/images/books", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    book.Images.Add(new BookImage { Url = $"/images/books/{fileName}" });
                }
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Book created successfully.", data = book.Id });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(long id, [FromForm] BookUpdateDto dto, [FromForm] List<IFormFile>? images, [FromForm] List<string>? deleteImages)
        {
            if (!IsAdmin()) return Forbid("Only admin users can update books.");

            var existing = await _context.Books
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existing == null)
                return NotFound(new { success = false, message = "Book not found." });

            if (!_context.Genres.Any(g => g.Id == dto.GenreId))
                return BadRequest(new { success = false, message = "Invalid Genre ID." });

            existing.Title = dto.Title;
            existing.Author = dto.Author;
            existing.GenreId = dto.GenreId;
            existing.Language = dto.Language;
            existing.Format = dto.Format;
            existing.Price = dto.Price;
            existing.PublicationDate = ToUtc(dto.PublicationDate);
            existing.IsPhysicalAccess = dto.IsPhysicalAccess;
            existing.Stock = dto.Stock;
            existing.Isbn = dto.Isbn;
            existing.Description = dto.Description;
            existing.Publisher = dto.Publisher;
            existing.BookType = dto.BookType;
            existing.IsExclusiveEdition = dto.IsExclusiveEdition;
            existing.IsOnSale = dto.IsOnSale;
            existing.DiscountPercentage = dto.DiscountPercentage;
            existing.DiscountStart = ToUtc(dto.DiscountStart);
            existing.DiscountEnd = ToUtc(dto.DiscountEnd);
            existing.UpdatedAt = DateTime.UtcNow;

            if (deleteImages != null)
            {
                var toDelete = existing.Images
                    .Where(img => deleteImages.Contains(img.Url))
                    .ToList();

                foreach (var img in toDelete)
                {
                    var path = Path.Combine("wwwroot", img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                    _context.BookImage.Remove(img);
                }
            }

            if (images != null)
            {
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine("wwwroot/images/books", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    existing.Images.Add(new BookImage { Url = $"/images/books/{fileName}" });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Book updated successfully." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(long id)
        {
            if (!IsAdmin())
                return Forbid("Only admin users can delete books.");

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { success = false, message = "Book not found." });

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Book deleted successfully." });
        }

        // ========== GENRES ==========

        [HttpGet("genres")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _context.Genres
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();

            return Ok(new { success = true, message = "Genres retrieved successfully.", data = genres });
        }

        [HttpGet("genres/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGenre(long id)
        {
            var genre = await _context.Genres
                .Where(g => g.Id == id)
                .Select(g => new { g.Id, g.Name })
                .FirstOrDefaultAsync();

            if (genre == null)
                return NotFound(new { success = false, message = "Genre not found." });

            return Ok(new { success = true, message = "Genre retrieved successfully.", data = genre });
        }

        [HttpPost("genres")]
        public async Task<IActionResult> CreateGenre([FromBody] Genre genre)
        {
            if (!IsAdmin())
                return Forbid("Only admin users can add genres.");

            if (string.IsNullOrWhiteSpace(genre.Name))
                return BadRequest(new { success = false, message = "Genre name is required." });

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Genre created successfully.", data = genre.Id });
        }

        [HttpPut("genres/{id}")]
        public async Task<IActionResult> UpdateGenre(long id, [FromBody] Genre genre)
        {
            if (!IsAdmin())
                return Forbid("Only admin users can update genres.");

            if (id != genre.Id)
                return BadRequest(new { success = false, message = "Genre ID mismatch." });

            var existing = await _context.Genres.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Genre not found." });

            existing.Name = genre.Name;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Genre updated successfully." });
        }

        [HttpDelete("genres/{id}")]
        public async Task<IActionResult> DeleteGenre(long id)
        {
            if (!IsAdmin())
                return Forbid("Only admin users can delete genres.");

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound(new { success = false, message = "Genre not found." });

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Genre deleted successfully." });
        }
        
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterBooks([FromQuery] BookFilterDto filter)
        {
            var query = _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(b =>
                    b.Title.Contains(filter.Search) ||
                    b.Author.Contains(filter.Search));
            }

            if (filter.GenreId.HasValue)
            {
                query = query.Where(b => b.GenreId == filter.GenreId);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(b => b.Price >= filter.MinPrice);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= filter.MaxPrice);
            }

            if (!string.IsNullOrEmpty(filter.Sort))
            {
                query = filter.Sort switch
                {
                    "price_desc" => query.OrderByDescending(b => b.Price),
                    "price_asc" => query.OrderBy(b => b.Price),
                    "title_asc" => query.OrderBy(b => b.Title),
                    "title_desc" => query.OrderByDescending(b => b.Title),
                    _ => query
                };
            }

            var books = await query.Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.Price,
                b.GenreId,
                GenreName = b.Genre.Name,
                b.Stock,
                b.Language,
                b.Format,
                b.PublicationDate,
                b.IsPhysicalAccess,
                b.IsOnSale,
                b.DiscountPercentage,
                b.DiscountStart,
                b.DiscountEnd,
                b.Isbn,
                b.Publisher,
                b.BookType,
                b.Description,
                b.IsExclusiveEdition,
                b.AverageRating,
                b.TotalSold,
                b.CreatedAt,
                b.UpdatedAt,
                Images = b.Images.Select(i => i.Url).ToList()
            }).ToListAsync();

            return Ok(new { data = books });
        }


    }
}
