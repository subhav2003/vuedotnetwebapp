using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly EmailService _emailService;

        public AccountController(AppDbContext context, JwtTokenHelper jwtTokenHelper, EmailService emailService)
        {
            _context = context;
            _jwtTokenHelper = jwtTokenHelper;
            _emailService = emailService; 
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
                Name = signup.Name,
                Username = signup.Username,
                Email = signup.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(signup.Password),
                Phone = signup.Phone,
                Gender = signup.Gender,
                DateOfBirth = signup.DateOfBirth.HasValue
                    ? DateTime.SpecifyKind(signup.DateOfBirth.Value, DateTimeKind.Utc)
                    : null,
                MembershipId = Guid.NewGuid().ToString(),
                MembershipStatus = "Active",
                DateOfRegistration = now,
                CreatedAt = now,
                UpdatedAt = now
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

        // === ADMIN SIGNUP ===
        [HttpPost("admin/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDto adminDto)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == adminDto.Email))
                return BadRequest(new { message = "Admin email already exists." });

            var now = DateTime.UtcNow;

            var admin = new Admin
            {
                Name = adminDto.Name,
                Email = adminDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(adminDto.Password),
                Phone = adminDto.Phone,
                Role = "admin",
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var token = _jwtTokenHelper.GenerateToken(admin.Id.ToString(), admin.Email, "admin");

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

        // === LOGIN ===
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var role = login.Role?.ToLower();

            if (role == "member")
            {
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

            if (role == "admin")
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == login.Email);

                if (admin == null || !BCrypt.Net.BCrypt.Verify(login.Password, admin.Password))
                    return Unauthorized(new { message = "Invalid email or password." });

                admin.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwtTokenHelper.GenerateToken(admin.Id.ToString(), admin.Email, "admin");

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

            return BadRequest(new { message = "Invalid role. Must be 'admin' or 'member'." });
        }
        // === Forget Password ===
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                var user = await _context.Members.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                    return NotFound(new { error = "Email not found" });

                var resetCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                var body = $"Hello,\n\nUse this code to reset your password: {resetCode}\n\n– Pustakalaya Team";

                await _emailService.SendEmailAsync(user.Email, "Reset Your Password", body);

                return Ok(new { message = "Reset code sent. Please check your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Something went wrong.", details = ex.Message });
            }
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

    public class AdminRegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }
}
