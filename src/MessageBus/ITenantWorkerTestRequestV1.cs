using System;

namespace YA.TenantWorker.MessageBus
{
    public interface ITenantWorkerTestRequestV1
    {
        DateTime Timestamp { get; }
    }
}
