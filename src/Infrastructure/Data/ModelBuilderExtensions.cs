using Microsoft.EntityFrameworkCore;
using System;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            Guid seedPricingTierId = Guid.Parse("00000000-0000-0000-0000-000000000003");

            modelBuilder.Entity<PricingTier>().HasData(
                new PricingTier
                {
                    PricingTierID = seedPricingTierId,
                    Title = "Бесплатный",
                    Description = "Бесплатно для всех."
                }
            );

            Guid seedTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            modelBuilder.Entity<Tenant>().HasData(
                new
                {
                    TenantID = seedTenantId,
                    TenantName = "Прохожий",
                    PricingTierID = seedPricingTierId,
                    TenantType = TenantTypes.Custom,
                    IsActive = true,
                    IsTrial = false,
                    IsReadOnly = false
                }
            );

            Guid seedAdminId = Guid.Parse("00000000-0000-0000-0000-000000000011");
            Guid seedUserId = Guid.Parse("00000000-0000-0000-0000-000000000012");

            modelBuilder.Entity<User>().HasData(
                new
                {
                    UserID = seedAdminId,
                    TenantID = seedTenantId,
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
                    TenantID = seedTenantId,
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