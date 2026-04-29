using Arboon.Domain.Entities;
using Arboon.Domain.Enums;
using Arboon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arboon.Infrastructure.Seeding;

/// <summary>
/// Seeds test data in Development mode for the MVP.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        // Only seed if no users exist
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("🌱 Seeding MVP mock data...");

        // 1. Seed Admin
        var adminUser = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            Name = "مدير النظام",
            Email = "admin@arboon.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        context.Users.Add(adminUser);

        var testSeller1 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "بائع تجريبي ١",
            Email = "seller1@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var testSeller2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "بائع تجريبي ٢",
            Email = "seller2@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234"),
            Role = "Seller",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        context.Users.AddRange(testSeller1, testSeller2);
        await context.SaveChangesAsync();

        // Create wallets for test sellers
        var wallet1 = new Wallet
        {
            UserId = testSeller1.Id,
            AvailableBalance = 1500,
            FrozenBalance = 500 // Matching the frozen escrow amount
        };
        
        var wallet2 = new Wallet
        {
            UserId = testSeller2.Id,
            AvailableBalance = 0,
            FrozenBalance = 0
        };

        context.Wallets.AddRange(wallet1, wallet2);
        await context.SaveChangesAsync();

        // Seed some escrows
        var pendingEscrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = testSeller1.Id,
            Title = "تصميم شعار وهوية بصرية",
            Amount = 1000,
            Currency = "SAR",
            Conditions = "يتم التسليم خلال أسبوع، ٣ تعديلات مجانية",
            Status = EscrowStatus.PENDING,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        pendingEscrow.CalculateAndSetFee();
        pendingEscrow.PaymentUrl = $"https://arboon.app/pay/{pendingEscrow.Id}";

        var frozenEscrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = testSeller1.Id,
            Title = "برمجة موقع تعريفي",
            Amount = 500,
            Currency = "SAR",
            Conditions = "موقع من ٥ صفحات مع لوحة تحكم",
            Status = EscrowStatus.FROZEN,
            BuyerEmail = "buyer1@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        frozenEscrow.CalculateAndSetFee();
        frozenEscrow.PaymentUrl = $"https://arboon.app/pay/{frozenEscrow.Id}";

        var releasedEscrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = testSeller2.Id,
            Title = "كتابة محتوى تسويقي",
            Amount = 300,
            Currency = "SAR",
            Conditions = "كتابة ١٠ مقالات متوافقة مع السيو",
            Status = EscrowStatus.RELEASED,
            BuyerEmail = "buyer2@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ReleasedAt = DateTime.UtcNow.AddDays(-1)
        };
        releasedEscrow.CalculateAndSetFee();
        releasedEscrow.PaymentUrl = $"https://arboon.app/pay/{releasedEscrow.Id}";

        var cancelledEscrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = testSeller1.Id,
            Title = "استشارة تسويقية لمدة ساعة",
            Amount = 200,
            Currency = "SAR",
            Conditions = "عبر زووم",
            Status = EscrowStatus.CANCELLED,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        cancelledEscrow.CalculateAndSetFee();
        cancelledEscrow.PaymentUrl = $"https://arboon.app/pay/{cancelledEscrow.Id}";

        var disputedEscrow = new Escrow
        {
            Id = Escrow.GenerateId(),
            SellerId = testSeller2.Id,
            Title = "تطوير تطبيق جوال",
            Amount = 2500,
            Currency = "SAR",
            Conditions = "تطبيق أندرويد و iOS",
            Status = EscrowStatus.DISPUTED,
            BuyerEmail = "disputed_buyer@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };
        disputedEscrow.CalculateAndSetFee();
        disputedEscrow.PaymentUrl = $"https://arboon.app/pay/{disputedEscrow.Id}";

        context.Escrows.AddRange(pendingEscrow, frozenEscrow, releasedEscrow, cancelledEscrow, disputedEscrow);
        await context.SaveChangesAsync();

        // Seed a dispute
        var dispute = new Dispute
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            EscrowId = disputedEscrow.Id,
            OpenedBy = DisputeParty.Buyer,
            Reason = "التطبيق به العديد من الأخطاء البرمجية ولم يتم الالتزام بالتصميم",
            Status = DisputeStatus.OPEN,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Disputes.Add(dispute);
        
        // Seed dispute messages
        var message1 = new DisputeMessage
        {
            DisputeId = dispute.Id,
            SenderType = DisputeParty.Buyer,
            Message = "الرجاء مراجعة المرفقات، التطبيق لا يعمل على نظام iOS",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var message2 = new DisputeMessage
        {
            DisputeId = dispute.Id,
            SenderType = DisputeParty.Seller,
            Message = "لقد قمت باختبار التطبيق وهو يعمل، المشكلة من جهازك",
            CreatedAt = DateTime.UtcNow.AddHours(-12)
        };
        context.DisputeMessages.AddRange(message1, message2);
        await context.SaveChangesAsync();

        // Seed webhook endpoint for seller 1
        var webhookEndpoint = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            SellerId = testSeller1.Id,
            Url = "https://webhook.site/placeholder-url", // Developer can change this later
            Secret = "whsec_test_secret",
            Events = "escrow.created,escrow.funded,escrow.released,dispute.opened",
            IsActive = true
        };
        context.WebhookEndpoints.Add(webhookEndpoint);
        await context.SaveChangesAsync();

        // Seed a buyer token for the frozen escrow so it can be tested for release
        var buyerToken = new BuyerToken
        {
            EscrowId = frozenEscrow.Id,
            Token = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        context.BuyerTokens.Add(buyerToken);
        await context.SaveChangesAsync();

        logger.LogInformation("✅ MVP mock data seeded successfully.");
        logger.LogInformation("Admin User: admin@arboon.app / Admin@123");
        logger.LogInformation("Seller 1: {Email} / Test@1234", testSeller1.Email);
        logger.LogInformation("Seller 2: {Email} / Test@1234", testSeller2.Email);
        logger.LogInformation("Frozen Escrow ID to test release: {EscrowId}", frozenEscrow.Id);
        logger.LogInformation("Buyer Token for testing release: {Token}", buyerToken.Token);
    }
}
