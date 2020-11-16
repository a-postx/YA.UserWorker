using System;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IRuntimeContextAccessor
    {
        Guid GetClientRequestId();
        Guid GetCorrelationId();
        Guid GetTenantId();
        string GetTraceId();
    }
}
