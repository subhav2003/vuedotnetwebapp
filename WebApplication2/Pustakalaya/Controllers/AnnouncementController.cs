using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Data;
using Pustakalaya.Dtos;
using Pustakalaya.Models;
using Pustakalaya.Services;
using System.Security.Claims;

namespace Pustakalaya.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PusherService _pusher;

        public AnnouncementController(AppDbContext context, PusherService pusher)
        {
            _context = context;
            _pusher = pusher;
        }

        private long GetMemberId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // === POST: /api/announcement ===
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromBody] AnnouncementCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { success = false, message = "Title and Message are required." });

            if (dto.StartDate == default || dto.EndDate == default)
                return BadRequest(new { success = false, message = "Start and End dates are required." });

            var model = new Announcement
            {
                MemberId = dto.MemberId, // null means public
                Title = dto.Title,
                Message = dto.Message,
                StartDate = dto.StartDate.Kind == DateTimeKind.Utc ? dto.StartDate : dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.Kind == DateTimeKind.Utc ? dto.EndDate : dto.EndDate.ToUniversalTime(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();

            var channel = model.MemberId.HasValue
                ? $"announcement.user.{model.MemberId}"
                : "announcement.public";

            await _pusher.TriggerAsync(channel, "NewAnnouncement", new
            {
                model.Id,
                model.Title,
                model.Message,
                model.StartDate,
                model.EndDate,
                model.IsActive,
                Audience = model.MemberId.HasValue ? $"user.{model.MemberId}" : "public"
            });

            return Ok(new
            {
                success = true,
                message = "Announcement created successfully.",
                data = model
            });
        }


        // === GET: /api/announcement/my ===
        [HttpGet("my")]
        public async Task<IActionResult> GetForCurrentUser()
        {
            var memberId = GetMemberId();

            var announcements = await _context.Announcements
                .Where(a =>
                    (a.MemberId == null || a.MemberId == memberId) &&
                    a.IsActive &&
                    (a.StartDate == null || a.StartDate <= DateTime.UtcNow) &&
                    (a.EndDate == null || a.EndDate >= DateTime.UtcNow)
                )
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Fetched latest announcements.",
                data = announcements
            });
        }

        // === GET: /api/announcement ===
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "All announcements fetched.",
                data = announcements
            });
        }

        // === GET: /api/announcement/{id} ===
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var announcement = await _context.Announcements.FindAsync(id);

            if (announcement == null)
                return NotFound(new { success = false, message = "Announcement not found." });

            return Ok(new
            {
                success = true,
                message = "Announcement found.",
                data = announcement
            });
        }
        
        // === PUT: /api/announcement/{id} ===
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(long id, [FromBody] AnnouncementCreateDto dto)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound(new { success = false, message = "Announcement not found." });

            announcement.Title = dto.Title;
            announcement.Message = dto.Message;
            announcement.StartDate = dto.StartDate.Kind == DateTimeKind.Utc ? dto.StartDate : dto.StartDate.ToUniversalTime();
            announcement.EndDate = dto.EndDate.Kind == DateTimeKind.Utc ? dto.EndDate : dto.EndDate.ToUniversalTime();
            announcement.IsActive = dto.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Announcement updated successfully.",
                data = announcement
            });
        }

        // === DELETE: /api/announcement/{id} ===
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(long id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound(new { success = false, message = "Announcement not found." });

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Announcement deleted successfully."
            });
        }

        // === GET: /api/announcement/public ===
        [Authorize(Roles = "admin")]
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicAnnouncements()
        {
            var publicAnnouncements = await _context.Announcements
                .Where(a => a.MemberId == null)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Public announcements fetched.",
                data = publicAnnouncements
            });
        }
        
        

    }
}
