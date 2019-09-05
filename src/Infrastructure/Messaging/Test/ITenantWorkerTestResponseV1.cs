using System;

namespace YA.TenantWorker.Infrastructure.Messaging.Test
{
    public interface ITenantWorkerTestResponseV1
    {
        DateTime GotIt { get; }
    }
}
