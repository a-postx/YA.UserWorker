using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.Configurations
{
    public class ApiRequestConfiguration : IEntityTypeConfiguration<ApiRequest>
    {
        public void Configure(EntityTypeBuilder<ApiRequest> modelBuilder)
        {
            modelBuilder.HasKey(k => k.ApiRequestID);

            modelBuilder.Property(p => p.ApiRequestDateTime)
               .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
        }
    }
}
