using System;

namespace YA.TenantWorker.Core.Entities
{
    public interface IAuditedEntityBase
    {
        DateTime CreatedDateTime { get; set; }
        DateTime LastModifiedDateTime { get; set; }
    }
}
