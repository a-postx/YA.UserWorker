using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Data.Configurations
{
    public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> modelBuilder)
        {
            modelBuilder.HasKey(m => new { m.MembershipID });

            modelBuilder.Property(p => p.CreatedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.LastModifiedDateTime)
                .HasDefaultValueSql(General.DefaultSqlModelDateTimeFunction)
                .ValueGeneratedOnAdd()
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.CreatedBy)
                .HasMaxLength(64);
            modelBuilder.Property(p => p.LastModifiedBy)
                .HasMaxLength(64);
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
        }
    }
}
