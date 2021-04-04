using System;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IRuntimeContextAccessor
    {
        Guid GetCorrelationId();
        (string authId, string userId) GetUserIdentifiers();
        Guid GetTenantId();
        string GetTraceId();
    }
}
