using Microsoft.EntityFrameworkCore;
using System;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Data
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
                    PricingTierID = Guid.Parse(SeedData.SeedPricingTierId),
                    Title = "Бесплатный",
                    Description = "Бесплатно для всех.",
                    HasTrial = false,
                    MaxUsers = 1
                },
                new PricingTier
                {
                    PricingTierID = paidPricingTierId,
                    Title = "Платный",
                    Description = "За денежки",
                    HasTrial = true,
                    TrialPeriod = TimeSpan.FromDays(15),
                    MaxUsers = 1
                }
            );

            Guid defaultTenantId = Guid.Parse(SeedData.SystemTenantId);
            Guid seedPaidTenantId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            modelBuilder.Entity<Tenant>().HasData(
                new
                {
                    TenantID = defaultTenantId,
                    Name = "Системный",
                    Type = TenantType.System,
                    Status = TenantStatus.Active,
                    PricingTierId = defaultPricingTierId,
                    PricingTierActivatedDateTime = DateTime.UtcNow,
                    PricingTierActivatedUntilDateTime = DateTime.MinValue,
                    IsReadOnly = false
                },
                new
                {
                    TenantID = seedPaidTenantId,
                    Name = "Уважаемый",
                    Type = TenantType.Custom,
                    Status = TenantStatus.Active,
                    PricingTierId = paidPricingTierId,
                    PricingTierActivatedDateTime = DateTime.UtcNow,
                    PricingTierActivatedUntilDateTime = DateTime.UtcNow.AddDays(30),
                    IsReadOnly = false
                }
            );

            Guid seedAdminId = Guid.Parse("00000000-0000-0000-0000-000000000012");
            Guid seedUserId = Guid.Parse("00000000-0000-0000-0000-000000000014");

            modelBuilder.Entity<User>().HasData(
                new
                {
                    UserID = seedAdminId,
                    Name = "Серый кардинал",
                    Email = "admin@email.com",
                    AuthProvider = "auth0",
                    ExternalId = "lahblah",
                    IsDeleted = false,
                    CreatedDateTime = DateTime.UtcNow,
                    LastModifiedDateTime = DateTime.UtcNow
                },
                new
                {
                    UserID = seedUserId,
                    Name = "Мышиный король",
                    Email = "user@email.com",
                    AuthProvider = "auth0",
                    ExternalId = "userLahblah",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.UtcNow,
                    LastModifiedDateTime = DateTime.UtcNow
                }
            );

            //добавление встроенных сущностей должно проходить после основной
            modelBuilder.Entity<User>().OwnsOne(e => e.Settings).HasData(
                new
                {
                    UserID = seedAdminId,
                    ShowGettingStarted = true
                },
                new
                {
                    UserID = seedUserId,
                    ShowGettingStarted = true
                }
            );

            Guid seedUserMembershipId = Guid.Parse("00000000-0000-0000-0000-000000000015");

            modelBuilder.Entity<Membership>().HasData(
                new
                {
                    MembershipID = seedUserMembershipId,
                    UserID = seedUserId,
                    TenantID = seedPaidTenantId,
                    AccessType = MembershipAccessType.Owner,
                    IsDeleted = false
                }
            );

        }
    }
}
