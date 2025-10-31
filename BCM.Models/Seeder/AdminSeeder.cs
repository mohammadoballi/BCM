using System;
using System.Linq;
using BCM.Models.Data;
using BCM.Models.Entites;
using BCM.Models.Enums;

namespace BCM.Models.Seeder
{
    public static class AdminSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();
            // Check if admin already exists
            if (!context.Users.Any(u => u.Role == Role.Admin))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@bcm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
                    Role = Role.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
                Console.WriteLine("✅ Admin user seeded successfully.");
            }
            else
            {
                Console.WriteLine("ℹ️ Admin user already exists, skipping seeding.");
            }
        }
    }
}
