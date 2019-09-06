﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.EntityConfigurations
{
    public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> modelBuilder)
        {
            modelBuilder.HasKey(k => new { k.PricingTierID });

            modelBuilder.Property(p => p.LastModifiedDateTime).HasDefaultValueSql(General.DefaultSqlModelChangeDateTime).ValueGeneratedOnAdd();
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
            modelBuilder.Property(p => p.Name)
                .IsUnicode()
                .HasMaxLength(128);
            modelBuilder.Property(p => p.Description)
                .IsUnicode()
                .HasMaxLength(128);
        }
    }
}
