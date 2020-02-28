using System;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IRuntimeContextAccessor
    {
        Guid GetCorrelationId();
        Guid GetTenantId();
    }
}
