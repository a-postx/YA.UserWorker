﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> modelBuilder)
        {
            modelBuilder.HasKey(m => new { m.UserID });

            modelBuilder.HasQueryFilter(f => !f.IsDeleted);

            modelBuilder.Property(p => p.CreatedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.LastModifiedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
            modelBuilder.Property(p => p.Username)
                .HasMaxLength(64)
                .IsRequired();
            modelBuilder.Property(p => p.FirstName)
                .IsUnicode()
                .HasMaxLength(255);
            modelBuilder.Property(p => p.LastName)
                .IsUnicode()
                .HasMaxLength(255);
            modelBuilder.Property(p => p.Email)
                .HasMaxLength(128);
            modelBuilder.Property(p => p.Password)
                .HasMaxLength(50);
        }
    }
}