using System;

namespace YA.TenantWorker.Messaging
{
    public interface ITenantWorkerTestRequestV1
    {
        DateTime Timestamp { get; }
    }
}
