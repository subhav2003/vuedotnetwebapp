using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pustakalaya.Models;

namespace Pustakalaya.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;

            var adminExists = await context.Admins.AnyAsync(a => a.Email == "admin@pustakalaya.com");
            if (!adminExists)
            {
                var admin = new Admin
                {
                    Name      = "Default Admin",
                    Email     = "admin@pustakalaya.com",
                    Phone     = "9800000000",
                    Role      = "admin",
                    Password  = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                context.Admins.Add(admin);
            }

            var superAdminExists = await context.Admins.AnyAsync(a => a.Email == "superadmin@pustakalaya.com");
            if (!superAdminExists)
            {
                var superAdmin = new Admin
                {
                    Name      = "Super Admin",
                    Email     = "superadmin@pustakalaya.com",
                    Phone     = "9800000001",
                    Role      = "superadmin",
                    Password  = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                context.Admins.Add(superAdmin);
            }

            await context.SaveChangesAsync();
        }
    }
}