using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data.Configurations
{
    public class ApiRequestConfiguration : IEntityTypeConfiguration<ApiRequest>
    {
        public void Configure(EntityTypeBuilder<ApiRequest> modelBuilder)
        {
            modelBuilder.HasKey(k => k.ApiRequestID);
            modelBuilder.Property(p => p.tstamp).IsRowVersion();
        }
    }
}
