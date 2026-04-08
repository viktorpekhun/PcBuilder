using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;

namespace PcBuilder.Api.Services
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var email = configuration["AdminSeed:Email"];
            var username = configuration["AdminSeed:Username"];
            var password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return;

            var context = serviceProvider.GetRequiredService<IApplicationDbContext>();

            var existingUser = await context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                return;

            var userRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "User");
            var adminRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");

            if (userRole == null || adminRole == null)
                return;

            var admin = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsEmailVerified = true,
                CommentBanUntil = DateTime.MinValue,
                PostBanUntil = DateTime.MinValue
            };

            admin.Roles.Add(userRole);
            admin.Roles.Add(adminRole);

            await context.Set<User>().AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
