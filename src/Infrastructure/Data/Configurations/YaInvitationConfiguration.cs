using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Data.Configurations;

public class YaInvitationConfiguration : IEntityTypeConfiguration<YaInvitation>
{
    public void Configure(EntityTypeBuilder<YaInvitation> modelBuilder)
    {
        modelBuilder.HasKey(k => new { k.YaInvitationID });

        modelBuilder.Property(p => p.YaInvitationID).ValueGeneratedOnAdd();
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

        modelBuilder.Property(p => p.Email)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Property(p => p.InvitedBy)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Property(p => p.AccessType)
            .IsRequired();
        modelBuilder.Property(p => p.Status)
            .IsRequired();
        modelBuilder.Property(p => p.ExpirationDate)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));
        modelBuilder.Property(p => p.Claimed)
            .HasDefaultValue(0);
        modelBuilder.Property(p => p.ClaimedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));
    }
}
