using System;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IRuntimeContextAccessor
    {
        Guid GetCorrelationId();
        string GetTraceId();

        Guid GetTenantId();
        
        (string authId, string userId) GetUserIdentifiers();
    }
}
