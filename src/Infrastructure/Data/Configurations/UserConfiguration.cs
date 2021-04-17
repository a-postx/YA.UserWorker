using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> modelBuilder)
        {
            modelBuilder.HasKey(m => new { m.UserID });

            modelBuilder.HasIndex(i => new { i.AuthProvider, i.ExternalId });

            //используется только в визуальных моделях, добавлено для сокращения количества запросов
            modelBuilder.Ignore(p => p.Tenants);

            OwnedNavigationBuilder<User, UserSetting> settingsBuilder = modelBuilder
                .OwnsOne(o => o.Settings);

            settingsBuilder.Property(p => p.ShowGettingStarted)
                .IsRequired();

            modelBuilder.Navigation(e => e.Settings).IsRequired();

            modelBuilder.Property(p => p.CreatedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.LastModifiedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
            modelBuilder.Property(p => p.Name)
                .HasMaxLength(64)
                .IsRequired();
            modelBuilder.Property(p => p.Email)
                .IsUnicode()
                .HasMaxLength(128);
            modelBuilder.Property(p => p.AuthProvider)
                .IsUnicode()
                .IsRequired()
                .HasMaxLength(128);
            modelBuilder.Property(p => p.ExternalId)
                .IsUnicode()
                .IsRequired()
                .HasMaxLength(128);
            modelBuilder.Property(p => p.Nickname)
                .IsUnicode()
                .HasMaxLength(128);
        }
    }
}
