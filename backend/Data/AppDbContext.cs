using Investly.API.Models.Entities;
using Investly.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Investly.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotificationRead> UserNotificationReads => Set<UserNotificationRead>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.Gender).HasConversion<string>().HasMaxLength(10);
            e.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.KycStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.MemberId).HasMaxLength(50);
            e.Property(u => u.Location).HasMaxLength(200);
            e.Property(u => u.PassportUrl).HasMaxLength(500);
            e.Property(u => u.CompanyName).HasMaxLength(200);
            e.Property(u => u.Avatar).HasMaxLength(500);
            e.Property(u => u.KycRejectionReason).HasMaxLength(500);
            e.HasIndex(u => u.Phone).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.MemberId).IsUnique();
            e.HasIndex(u => u.Role);
            e.HasIndex(u => u.Status);
            e.HasCheckConstraint("chk_user_age", "age >= 18 AND age <= 120");
        });

        // Wallet
        modelBuilder.Entity<Wallet>(e =>
        {
            e.ToTable("wallets");
            e.HasKey(w => w.Id);
            e.Property(w => w.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(w => w.Balance).HasColumnType("numeric(18,2)");
            e.Property(w => w.TotalDeposits).HasColumnType("numeric(18,2)");
            e.Property(w => w.TotalWithdrawals).HasColumnType("numeric(18,2)");
            e.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(w => w.UserId).IsUnique();
            e.HasCheckConstraint("chk_wallet_balance", "balance >= 0");
            e.HasOne(w => w.User).WithOne(u => u.Wallet).HasForeignKey<Wallet>(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasMaxLength(20);
            e.Property(c => c.NameAr).HasMaxLength(50).IsRequired();
            e.Property(c => c.NameEn).HasMaxLength(50).IsRequired();
            e.Property(c => c.Icon).HasMaxLength(50);

            e.HasData(
                new Category { Id = "tech", NameAr = "تقنية", NameEn = "Technology", Icon = "laptop-outline" },
                new Category { Id = "energy", NameAr = "طاقة متجددة", NameEn = "Renewable Energy", Icon = "sunny-outline" },
                new Category { Id = "agri", NameAr = "زراعة", NameEn = "Agriculture", Icon = "leaf-outline" },
                new Category { Id = "health", NameAr = "صحة", NameEn = "Health", Icon = "medkit-outline" },
                new Category { Id = "edu", NameAr = "تعليم", NameEn = "Education", Icon = "school-outline" },
                new Category { Id = "realestate", NameAr = "عقارات", NameEn = "Real Estate", Icon = "home-outline" }
            );
        });

        // Project
        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(p => p.TitleAr).HasMaxLength(200).IsRequired();
            e.Property(p => p.TitleEn).HasMaxLength(200).IsRequired();
            e.Property(p => p.CategoryId).HasColumnName("category").HasMaxLength(20);
            e.Property(p => p.CityAr).HasMaxLength(100);
            e.Property(p => p.CityEn).HasMaxLength(100);
            e.Property(p => p.ImageUrl).HasMaxLength(500);
            e.Property(p => p.Goal).HasColumnType("numeric(18,2)");
            e.Property(p => p.Raised).HasColumnType("numeric(18,2)");
            e.Property(p => p.MinInvestment).HasColumnType("numeric(18,2)");
            e.Property(p => p.MaxInvestment).HasColumnType("numeric(18,2)");
            e.Property(p => p.CurrencyCode).HasMaxLength(10).HasDefaultValue("LYD");
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Reference).HasMaxLength(50);
            e.Property(p => p.Website).HasMaxLength(500);
            e.Property(p => p.FounderName).HasMaxLength(100);
            e.Property(p => p.FounderEmail).HasMaxLength(150);
            e.Property(p => p.FounderPhone).HasMaxLength(20);
            e.Property(p => p.RejectionReason).HasMaxLength(500);
            e.HasIndex(p => p.Reference).IsUnique();
            e.HasIndex(p => p.OwnerId);
            e.HasIndex(p => p.Status);
            e.HasIndex(p => p.CategoryId);
            e.HasIndex(p => p.IsFeatured);
            e.HasCheckConstraint("chk_project_goal", "goal > 0");
            e.HasCheckConstraint("chk_project_raised", "raised >= 0");
            e.HasOne(p => p.Category).WithMany(c => c.Projects).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Owner).WithMany(u => u.Projects).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Investment
        modelBuilder.Entity<Investment>(e =>
        {
            e.ToTable("investments");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(i => i.Amount).HasColumnType("numeric(18,2)");
            e.Property(i => i.CurrencyCode).HasMaxLength(10).HasDefaultValue("LYD");
            e.Property(i => i.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            e.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(i => i.Reference).HasMaxLength(50);
            e.Property(i => i.Notes).HasMaxLength(500);
            e.HasIndex(i => i.Reference).IsUnique();
            e.HasIndex(i => i.ProjectId);
            e.HasIndex(i => i.InvestorId);
            e.HasIndex(i => i.Status);
            e.HasCheckConstraint("chk_investment_amount", "amount > 0");
            e.HasOne(i => i.Project).WithMany(p => p.Investments).HasForeignKey(i => i.ProjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.Investor).WithMany(u => u.Investments).HasForeignKey(i => i.InvestorId).OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(p => p.Amount).HasColumnType("numeric(18,2)");
            e.Property(p => p.CurrencyCode).HasMaxLength(10).HasDefaultValue("LYD");
            e.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.TransactionId).HasMaxLength(100);
            e.Property(p => p.Notes).HasMaxLength(500);
            e.Property(p => p.RejectedReason).HasMaxLength(500);
            e.HasIndex(p => p.TransactionId).IsUnique();
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => p.Status);
            e.HasCheckConstraint("chk_payment_amount", "amount > 0");
            e.HasOne(p => p.User).WithMany(u => u.Payments).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Investment).WithMany(i => i.Payments).HasForeignKey(p => p.InvestmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Approver).WithMany().HasForeignKey(p => p.ApprovedBy).OnDelete(DeleteBehavior.SetNull);
        });

        // WalletTransaction
        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.ToTable("wallet_transactions");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.Type).HasConversion<string>().HasMaxLength(10);
            e.Property(t => t.Amount).HasColumnType("numeric(18,2)");
            e.Property(t => t.CurrencyCode).HasMaxLength(10).HasDefaultValue("LYD");
            e.Property(t => t.TitleAr).HasMaxLength(200);
            e.Property(t => t.TitleEn).HasMaxLength(200);
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.Reference).HasMaxLength(100);
            e.Property(t => t.AdminNote).HasMaxLength(500);
            e.HasIndex(t => t.WalletId);
            e.HasIndex(t => t.UserId);
            e.HasIndex(t => t.CreatedAt);
            e.HasCheckConstraint("chk_wallet_tx_amount", "amount > 0");
            e.HasOne(t => t.Wallet).WithMany(w => w.Transactions).HasForeignKey(t => t.WalletId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.User).WithMany(u => u.WalletTransactions).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(n => n.TitleAr).HasMaxLength(200);
            e.Property(n => n.TitleEn).HasMaxLength(200);
            e.Property(n => n.MessageAr).IsRequired();
            e.Property(n => n.MessageEn).IsRequired();
            e.HasIndex(n => n.TargetUserId);
            e.HasOne(n => n.TargetUser).WithMany().HasForeignKey(n => n.TargetUserId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(n => n.Sender).WithMany(u => u.SentNotifications).HasForeignKey(n => n.SentBy).OnDelete(DeleteBehavior.SetNull);
        });

        // UserNotificationRead
        modelBuilder.Entity<UserNotificationRead>(e =>
        {
            e.ToTable("user_notification_reads");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(r => new { r.NotificationId, r.UserId }).IsUnique();
            e.HasIndex(r => r.UserId);
            e.HasOne(r => r.Notification).WithMany(n => n.Reads).HasForeignKey(r => r.NotificationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.User).WithMany(u => u.NotificationReads).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // NotificationSettings
        modelBuilder.Entity<NotificationSettings>(e =>
        {
            e.ToTable("notification_settings");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(s => s.UserId).IsUnique();
            e.HasOne(s => s.User).WithOne(u => u.NotificationSettings).HasForeignKey<NotificationSettings>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(t => t.Token).IsUnique();
            e.HasIndex(t => t.UserId);
            e.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // OtpCode
        modelBuilder.Entity<OtpCode>(e =>
        {
            e.ToTable("otp_codes");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(o => o.Phone).HasMaxLength(20).IsRequired();
            e.Property(o => o.Code).HasMaxLength(10).IsRequired();
            e.Property(o => o.Purpose).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(o => o.Phone);
        });

        // PasswordResetCode
        modelBuilder.Entity<PasswordResetCode>(e =>
        {
            e.ToTable("password_reset_codes");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(p => p.Email).HasMaxLength(150).IsRequired();
            e.Property(p => p.Code).HasMaxLength(10).IsRequired();
            e.HasIndex(p => p.Email);
        });

        // Media
        modelBuilder.Entity<Media>(e =>
        {
            e.ToTable("media");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(m => m.Url).HasMaxLength(500).IsRequired();
            e.Property(m => m.FileName).HasMaxLength(200);
            e.Property(m => m.FileType).HasMaxLength(50);
            e.HasIndex(m => m.UploadedBy);
            e.HasIndex(m => m.CreatedAt);
            e.HasOne(m => m.Uploader).WithMany(u => u.UploadedMedia).HasForeignKey(m => m.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        });

        // AppSetting — singleton row (Id = 1) for admin-controlled remote settings.
        modelBuilder.Entity<AppSetting>(e =>
        {
            e.ToTable("app_settings");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedNever();
            e.Property(s => s.MaintenanceMessageAr).HasMaxLength(500);
            e.Property(s => s.MaintenanceMessageEn).HasMaxLength(500);
            e.Property(s => s.AnnouncementAr).HasMaxLength(500);
            e.Property(s => s.AnnouncementEn).HasMaxLength(500);
            e.Property(s => s.MinSupportedVersion).HasMaxLength(20);

            e.HasData(new AppSetting
            {
                Id = 1,
                MaintenanceMode = false,
                MaintenanceMessageAr = "التطبيق قيد الصيانة حالياً. يرجى المحاولة لاحقاً.",
                MaintenanceMessageEn = "The app is under maintenance. Please try again later.",
                AnnouncementActive = false,
                AnnouncementAr = string.Empty,
                AnnouncementEn = string.Empty,
                AllowRegistration = true,
                AllowInvestments = true,
                MinSupportedVersion = string.Empty,
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });
    }
}
