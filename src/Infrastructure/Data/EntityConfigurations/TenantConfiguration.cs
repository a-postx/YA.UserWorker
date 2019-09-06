﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.EntityConfigurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> modelBuilder)
        {
            modelBuilder.HasKey(k => k.TenantID);

            modelBuilder.Property(p => p.LastModifiedDateTime).HasDefaultValueSql(General.DefaultSqlModelChangeDateTime).ValueGeneratedOnAdd();
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
            modelBuilder.Property(p => p.TenantName)
                .IsUnicode()
                .HasMaxLength(128);

            modelBuilder
                .HasMany(c => c.Users)
                .WithOne(u => u.Tenant)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder
                .HasOne(c => c.PricingTier)
                .WithMany(u => u.Tenants)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
