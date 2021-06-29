using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> modelBuilder)
        {
            modelBuilder.HasKey(k => k.TenantID);

            modelBuilder.Property(p => p.CreatedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.LastModifiedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.PricingTierActivatedDateTime)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.PricingTierActivatedUntilDateTime)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.tstamp).IsRowVersion();

            modelBuilder
                .HasMany(c => c.Invitations)
                .WithOne(u => u.Tenant)
                .HasForeignKey(k => k.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder
                .HasMany(c => c.Memberships)
                .WithOne(u => u.Tenant)
                .HasForeignKey(k => k.TenantID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Property(p => p.Name)
                .IsUnicode()
                .HasMaxLength(128);
        }
    }
}
