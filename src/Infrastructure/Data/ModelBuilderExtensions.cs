using Microsoft.EntityFrameworkCore;
using System;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            Guid defaultPricingTierId = Guid.Parse(SeedData.SeedPricingTierId);
            Guid paidPricingTierId = Guid.Parse("00000000-0000-0000-0000-000000000013");

            modelBuilder.Entity<PricingTier>().HasData(
                new PricingTier
                {
                    PricingTierID = defaultPricingTierId,
                    Title = "Бесплатный",
                    Description = "Бесплатно для всех.",
                    HasTrial = false,
                    MaxUsers = 1,
                    MaxVkCommunities = 1,
                    MaxVkCommunitySize = 1000,
                    MaxScheduledTasks = 0
                },
                new PricingTier
                {
                    PricingTierID = paidPricingTierId,
                    Title = "Платный",
                    Description = "За денежки.",
                    HasTrial = true,
                    TrialPeriod = TimeSpan.FromDays(15),
                    MaxUsers = 1,
                    MaxVkCommunities = 1,
                    MaxVkCommunitySize = 10000,
                    MaxScheduledTasks = 1
                }
            );

            Guid defaultTenantId = Guid.Parse(SeedData.SystemTenantId);
            Guid seedPaidTenantId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            modelBuilder.Entity<Tenant>().HasData(
                new
                {
                    TenantID = defaultTenantId,
                    TenantName = "Прохожий",
                    PricingTierID = defaultPricingTierId,
                    PricingTierActivatedDateTime = DateTime.UtcNow,
                    TenantType = TenantTypes.System,
                    IsActive = true,
                    IsReadOnly = false
                },
                new
                {
                    TenantID = seedPaidTenantId,
                    TenantName = "Уважаемый",
                    PricingTierID = paidPricingTierId,
                    PricingTierActivatedDateTime = DateTime.UtcNow,
                    TenantType = TenantTypes.Custom,
                    IsActive = true,
                    IsReadOnly = false
                }
            );

            Guid seedAdminId = Guid.Parse("00000000-0000-0000-0000-000000000011");
            Guid seedUserId = Guid.Parse("00000000-0000-0000-0000-000000000012");

            modelBuilder.Entity<User>().HasData(
                new
                {
                    UserID = seedAdminId,
                    TenantID = defaultTenantId,
                    Username = "admin@ya.ru",
                    Password = "123",
                    FirstName = "My",
                    LastName = "Admin",
                    Email = "admin@email.com",
                    Role = "Administrator",
                    IsActive = true,
                    IsPendingActivation = false,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.UtcNow,
                    LastModifiedDateTime = DateTime.UtcNow
                },
                new
                {
                    UserID = seedUserId,
                    TenantID = defaultTenantId,
                    Username = "user@ya.ru",
                    Password = "123",
                    FirstName = "My",
                    LastName = "User",
                    Email = "user@email.com",
                    Role = "User",
                    IsActive = true,
                    IsPendingActivation = false,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.UtcNow,
                    LastModifiedDateTime = DateTime.UtcNow
                }
            );
        }
    }
}