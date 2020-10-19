using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.Configurations
{
    public class ClientInfoConfiguration : IEntityTypeConfiguration<YaClientInfo>
    {
        public void Configure(EntityTypeBuilder<YaClientInfo> modelBuilder)
        {
            modelBuilder.HasKey(k => k.YaClientInfoID);

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
                .IsUnicode()
                .HasMaxLength(320);
            modelBuilder.Property(p => p.IpAddress)
                .IsUnicode()
                .HasMaxLength(50);
            modelBuilder.Property(p => p.CountryName)
                .IsUnicode()
                .HasMaxLength(500);
            modelBuilder.Property(p => p.RegionName)
                .IsUnicode()
                .HasMaxLength(500);
            modelBuilder.Property(p => p.DeviceModel)
                .IsUnicode()
                .HasMaxLength(500);
            modelBuilder.Property(p => p.Os)
                .IsUnicode()
                .HasMaxLength(200);
            modelBuilder.Property(p => p.OsVersion)
                .IsUnicode()
                .HasMaxLength(50);
            modelBuilder.Property(p => p.Browser)
                .IsUnicode()
                .HasMaxLength(500);
            modelBuilder.Property(p => p.BrowserVersion)
                .IsUnicode()
                .HasMaxLength(50);
            modelBuilder.Property(p => p.ScreenResolution)
                .IsUnicode()
                .HasMaxLength(50);
            modelBuilder.Property(p => p.ViewportSize)
                .IsUnicode()
                .HasMaxLength(50);
        }
    }
}
