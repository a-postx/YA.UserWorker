using System;

namespace YA.TenantWorker.Messaging.Test
{
    public interface ITenantWorkerTestResponseV1
    {
        DateTime GotIt { get; }
    }
}
