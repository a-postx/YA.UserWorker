using System;

namespace YA.TenantWorker.Messaging.Test
{
    public interface ITenantWorkerTestRequestV1
    {
        DateTime Timestamp { get; }
    }
}
