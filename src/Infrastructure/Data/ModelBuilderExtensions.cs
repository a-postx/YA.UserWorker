using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YA.TenantWorker.Constants;
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
                    IsReadOnly = false
                }
            );
        }
    }
}