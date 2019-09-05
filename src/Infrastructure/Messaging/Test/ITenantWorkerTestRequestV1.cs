using System;

namespace YA.TenantWorker.Infrastructure.Messaging.Test
{
    public interface ITenantWorkerTestRequestV1
    {
        DateTime Timestamp { get; }
    }
}
