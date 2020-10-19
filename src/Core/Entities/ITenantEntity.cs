using System;

namespace YA.TenantWorker.Core.Entities
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
        Tenant Tenant { get; set; }
    }
}
