using System;

namespace YA.UserWorker.Infrastructure.Messaging.Test
{
    public interface IUserWorkerTestRequestV1
    {
        DateTime Timestamp { get; }
    }
}
