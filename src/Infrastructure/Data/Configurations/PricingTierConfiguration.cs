using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.Configurations
{
    public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> modelBuilder)
        {
            modelBuilder.HasKey(k => new { k.PricingTierID });

            modelBuilder.Property(p => p.CreatedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.LastModifiedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.TrialPeriod)
                .HasConversion(new TimeSpanToTicksConverter());
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
            modelBuilder.Property(p => p.Title)
                .IsUnicode()
                .HasMaxLength(128);
            modelBuilder.Property(p => p.Description)
                .IsUnicode()
                .HasMaxLength(128);

            modelBuilder
                .HasMany(c => c.Tenants)
                .WithOne(u => u.PricingTier)
                .HasForeignKey(k => k.PricingTierId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
