using System;

namespace YA.TenantWorker.Application.Runtime
{
    internal class MbMessageContext
    {
        internal Guid CorrelationId { get; set; }
        internal Guid TenantId { get; set; }
    }
}
