using System;

namespace YA.TenantWorker.MessageBus
{
    public interface ITenantWorkerTestResponseV1
    {
        DateTime GotIt { get; }
    }
}
