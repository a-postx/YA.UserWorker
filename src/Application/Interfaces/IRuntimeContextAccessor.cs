using System;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IRuntimeContextAccessor
    {
        Guid GetCorrelationId();
        string GetTraceId();

        string GetUserId();
        Guid GetTenantId();
        
        (string authId, string userId) GetUserIdentifiers();
        
    }
}
