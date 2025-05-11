using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using Pustakalaya.Data;
using Pustakalaya.Helpers;
using Pustakalaya.Models;

namespace Pustakalaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public AccountController(AppDbContext context, JwtTokenHelper jwtTokenHelper)
        {
            _context = context;
            _jwtTokenHelper = jwtTokenHelper;
        }

        // === MEMBER SIGNUP ===
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDto signup)
        {
            if (signup.Role?.ToLower() != "member")
                return BadRequest(new { message = "Only 'member' registration allowed from this route." });

            if (await _context.Members.AnyAsync(m => m.Email == signup.Email))
                return BadRequest(new { message = "Member email already exists." });

            if (await _context.Members.AnyAsync(m => m.Username == signup.Username))
                return BadRequest(new { message = "Username already taken." });

            var now = DateTime.UtcNow;
            var member = new Member
            {
                Name               = signup.Name,
                Username           = signup.Username,
                Email              = signup.Email,
                Password           = BCrypt.Net.BCrypt.HashPassword(signup.Password),
                Phone              = signup.Phone,
                Gender             = signup.Gender,
                DateOfBirth        = signup.DateOfBirth.HasValue
                                        ? DateTime.SpecifyKind(signup.DateOfBirth.Value, DateTimeKind.Utc)
                                        : (DateTime?)null,
                MembershipId       = Guid.NewGuid().ToString(),
                MembershipStatus   = "Active",
                DateOfRegistration = now,
                CreatedAt          = now,
                UpdatedAt          = now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var token = _jwtTokenHelper.GenerateToken(member.Id.ToString(), member.Email, "member");
            var user = new
            {
                member.Id,
                member.Name,
                member.Username,
                member.Email,
                member.Phone,
                member.Gender,
                member.DateOfBirth,
                member.DateOfRegistration,
                member.MembershipId,
                member.MembershipStatus,
                member.LastLogin,
                member.CreatedAt,
                member.UpdatedAt,
                role = "member"
            };

            return Ok(new { token, user });
        }

        // === LOGIN ===
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var role = login.Role?.ToLower();
            if (role != "member")
                return BadRequest(new { message = "Invalid role. Must be 'member'." });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == login.Email);
            if (member == null || !BCrypt.Net.BCrypt.Verify(login.Password, member.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            member.LastLogin = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtTokenHelper.GenerateToken(member.Id.ToString(), member.Email, "member");
            var user = new
            {
                member.Id,
                member.Name,
                member.Username,
                member.Email,
                member.Phone,
                member.Gender,
                member.DateOfBirth,
                member.DateOfRegistration,
                member.MembershipId,
                member.MembershipStatus,
                member.LastLogin,
                member.CreatedAt,
                member.UpdatedAt,
                role = "member"
            };

            return Ok(new { token, user });
        }

        // === GET PROFILE ===
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var userId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == userId);
            if (member == null)
                return NotFound(new { message = "User not found." });

            var dto = new ProfileDto
            {
                Id                 = member.Id,
                Name               = member.Name,
                Username           = member.Username,
                Email              = member.Email,
                Phone              = member.Phone,
                Gender             = member.Gender,
                DateOfBirth        = member.DateOfBirth?.ToString("yyyy-MM-dd"),
                DateOfRegistration = member.DateOfRegistration.ToString("yyyy-MM-dd"),
                MembershipId       = member.MembershipId,
                MembershipStatus   = member.MembershipStatus,
                LastLogin          = member.LastLogin?.ToString("o")
            };

            // Wrap in `user` so front-end expects data.user
            return Ok(new { user = dto });
        }

        // === UPDATE PROFILE ===
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out var userId))
                return Unauthorized(new { message = "Invalid token." });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == userId);
            if (member == null)
                return NotFound(new { message = "User not found." });

            // Unique checks
            if (!string.Equals(dto.Email, member.Email, StringComparison.OrdinalIgnoreCase)
                && await _context.Members.AnyAsync(m => m.Email == dto.Email && m.Id != userId))
                return BadRequest(new { message = "Email already in use." });

            if (!string.Equals(dto.Username, member.Username, StringComparison.OrdinalIgnoreCase)
                && await _context.Members.AnyAsync(m => m.Username == dto.Username && m.Id != userId))
                return BadRequest(new { message = "Username already taken." });

            // Apply updates
            member.Name        = dto.Name;
            member.Username    = dto.Username;
            member.Email       = dto.Email;
            member.Phone       = dto.Phone;
            member.Gender      = dto.Gender;
            member.DateOfBirth = dto.DateOfBirth.HasValue
                ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            // Optional password change
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password != dto.PasswordConfirmation)
                    return BadRequest(new { message = "Passwords do not match." });
                member.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updatedDto = new ProfileDto
            {
                Id                 = member.Id,
                Name               = member.Name,
                Username           = member.Username,
                Email              = member.Email,
                Phone              = member.Phone,
                Gender             = member.Gender,
                DateOfBirth        = member.DateOfBirth?.ToString("yyyy-MM-dd"),
                DateOfRegistration = member.DateOfRegistration.ToString("yyyy-MM-dd"),
                MembershipId       = member.MembershipId,
                MembershipStatus   = member.MembershipStatus,
                LastLogin          = member.LastLogin?.ToString("o")
            };

            return Ok(new { message = "Profile updated successfully.", user = updatedDto });
        }
    }

    // === DTOs ===
    public class SignupDto
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ProfileDto
    {
        public long   Id                 { get; set; }
        public string Name               { get; set; } = default!;
        public string Username           { get; set; } = default!;
        public string Email              { get; set; } = default!;
        public string Phone              { get; set; } = default!;
        public string Gender             { get; set; } = default!;
        public string? DateOfBirth       { get; set; }
        public string DateOfRegistration { get; set; } = default!;
        public string MembershipId       { get; set; } = default!;
        public string MembershipStatus   { get; set; } = default!;
        public string? LastLogin         { get; set; }
    }

    public class UpdateProfileDto
    {
        public string Name                  { get; set; } = default!;
        public string Username              { get; set; } = default!;
        public string Email                 { get; set; } = default!;
        public string Phone                 { get; set; } = default!;
        public string Gender                { get; set; } = default!;
        public DateTime? DateOfBirth        { get; set; }
        public string? Password             { get; set; }
        public string? PasswordConfirmation { get; set; }
    }
}
