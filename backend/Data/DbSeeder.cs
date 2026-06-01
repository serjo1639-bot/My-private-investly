using Investly.API.Helpers;
using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.admin))
            return;

        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            Name = "Platform Admin",
            Phone = "+218910000001",
            Email = "admin@investly.ly",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@2024!"),
            Role = UserRole.admin,
            MemberId = ReferenceGenerator.MemberId("+218910000001"),
            Status = UserStatus.active,
            KycStatus = KycStatus.approved
        };

        var adminWallet = new Wallet { UserId = adminId };
        var adminSettings = new NotificationSettings { UserId = adminId };

        db.Users.Add(admin);
        db.Wallets.Add(adminWallet);
        db.NotificationSettings.Add(adminSettings);

        await db.SaveChangesAsync();
    }
}
