using System;

namespace YA.TenantWorker.Messaging
{
    public interface ITenantWorkerTestResponseV1
    {
        DateTime GotIt { get; }
    }
}
