using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Pustakalaya.Data;
using Pustakalaya.Helpers;
using Pustakalaya.Models;
using System;
using System.Threading.Tasks;

namespace Pustakalaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext   _context;
        private readonly JwtTokenHelper _jwt;
        private readonly EmailService   _emailSvc;

        public AccountController(AppDbContext   context,
                                 JwtTokenHelper jwt,
                                 EmailService   emailSvc)
        {
            _context  = context;
            _jwt      = jwt;
            _emailSvc = emailSvc;
        }

    // ───────────────────────────────────────── SIGN‑UP ─────────────────────────────────────────

        [HttpPost("signup")]                  // MEMBER
        public async Task<IActionResult> Signup([FromBody] SignupDto dto)
        {
            if (dto.Role?.ToLower() != "member")
                return BadRequest(new { message = "Only 'member' registration allowed from this route." });

            if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
                return BadRequest(new { message = "Member email already exists." });

            if (await _context.Members.AnyAsync(m => m.Username == dto.Username))
                return BadRequest(new { message = "Username already taken." });

            var now = DateTime.UtcNow;

            var member = new Member
            {
                Name               = dto.Name,
                Username           = dto.Username,
                Email              = dto.Email,
                Password           = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone              = dto.Phone,
                Gender             = dto.Gender,
                DateOfBirth        = dto.DateOfBirth?.ToUniversalTime(),
                MembershipId       = Guid.NewGuid().ToString(),
                MembershipStatus   = "Active",
                DateOfRegistration = now,
                CreatedAt          = now,
                UpdatedAt          = now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var token = _jwt.GenerateToken(member.Id.ToString(), member.Email, "member");

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

        [HttpPost("admin/register")]          // ADMIN SIGN‑UP
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDto dto)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                return BadRequest(new { message = "Admin email already exists." });

            var admin = new Admin
            {
                Name      = dto.Name,
                Email     = dto.Email,
                Password  = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone     = dto.Phone,
                Role      = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var token = _jwt.GenerateToken(admin.Id.ToString(), admin.Email, "admin");

            var user = new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Phone,
                admin.CreatedAt,
                admin.UpdatedAt,
                role = "admin"
            };

            return Ok(new { token, user });
        }

    // ───────────────────────────────────────── LOGIN ───────────────────────────────────────────

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var role = dto.Role?.ToLower();

            if (role == "member")
            {
                var m = await _context.Members.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (m == null || !BCrypt.Net.BCrypt.Verify(dto.Password, m.Password))
                    return Unauthorized(new { message = "Invalid email or password." });

                m.LastLogin = m.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwt.GenerateToken(m.Id.ToString(), m.Email, "member");

                var user = new
                {
                    m.Id, m.Name, m.Username, m.Email, m.Phone, m.Gender,
                    m.DateOfBirth, m.DateOfRegistration, m.MembershipId,
                    m.MembershipStatus, m.LastLogin, m.CreatedAt, m.UpdatedAt,
                    role = "member"
                };
                return Ok(new { token, user });
            }

            if (role == "admin")
            {
                var a = await _context.Admins.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (a == null || !BCrypt.Net.BCrypt.Verify(dto.Password, a.Password))
                    return Unauthorized(new { message = "Invalid email or password." });

                a.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwt.GenerateToken(a.Id.ToString(), a.Email, "admin");

                var user = new
                {
                    a.Id, a.Name, a.Email, a.Phone,
                    a.CreatedAt, a.UpdatedAt,
                    role = "admin"
                };
                return Ok(new { token, user });
            }

            return BadRequest(new { message = "Invalid role. Must be 'admin' or 'member'." });
        }

    // ────────────────────────────────── FORGOT PASSWORD ─────────────────────────────────────────

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var member = await _context.Members.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (member == null) return NotFound(new { error = "Email not found" });

            var code = Guid.NewGuid().ToString("N")[..8].ToUpper();
            var body = $"Hello,\n\nUse this code to reset your password: {code}\n\n– Pustakalaya Team";
            await _emailSvc.SendEmailAsync(member.Email, "Reset Your Password", body);

            return Ok(new { message = "Reset code sent. Please check your email." });
        }

    // ─────────────────────────────── GET AUTHENTICATED PROFILE ──────────────────────────────────

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // 1. read the user id from the token
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ??  User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Invalid token user id." });

        var role = User.FindFirstValue(ClaimTypes.Role);

        // 2. query the correct table with a long key
        if (role == "member")
        {
            var member = await _context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == userId);
            if (member == null)
                return NotFound(new { message = "Member not found." });

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
            return Ok(new { user });
        }

        if (role == "admin")
        {
            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == userId);
            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            var user = new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Phone,
                admin.CreatedAt,
                admin.UpdatedAt,
                role = "admin"
            };
            return Ok(new { user });
        }

        return BadRequest(new { message = "Unsupported role." });
    }

    // ─────────────────────────────────── DTO CLASSES ────────────────────────────────────────────
        public class SignupDto
        {
            public string   Name         { get; set; } = string.Empty;
            public string   Username     { get; set; } = string.Empty;
            public string   Email        { get; set; } = string.Empty;
            public string   Password     { get; set; } = string.Empty;
            public string   Phone        { get; set; } = string.Empty;
            public string   Gender       { get; set; } = string.Empty;
            public DateTime? DateOfBirth { get; set; }
            public string   Role         { get; set; } = string.Empty;
        }

        public class AdminRegisterDto
        {
            public string Name     { get; set; } = string.Empty;
            public string Email    { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Phone    { get; set; } = string.Empty;
        }

        public class LoginDto
        {
            public string Email    { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role     { get; set; } = string.Empty;
        }

        public class ForgotPasswordDto
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
