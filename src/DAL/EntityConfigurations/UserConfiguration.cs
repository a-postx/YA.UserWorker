using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Models;

namespace YA.TenantWorker.DAL.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> modelBuilder)
        {
            modelBuilder.HasKey(m => new { m.UserID });

            modelBuilder.HasQueryFilter(f => !f.IsDeleted);

            modelBuilder.Property(p => p.CreatedDateTime).HasDefaultValueSql(General.DefaultSqlModelChangeDateTime).ValueGeneratedOnAdd();
            modelBuilder.Property(p => p.LastModifiedDateTime).HasDefaultValueSql(General.DefaultSqlModelChangeDateTime).ValueGeneratedOnUpdate();
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
        }
    }
}
