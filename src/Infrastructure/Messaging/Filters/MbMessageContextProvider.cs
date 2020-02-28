using GreenPipes;
using MassTransit;
using MbEvents;
using System;
using YA.TenantWorker.Application.Runtime;

namespace YA.TenantWorker.Infrastructure.Messaging.Filters
{
    /// <summary>
    /// Provides tenant context from message bus message.
    /// </summary>
    internal static class MbMessageContextProvider
    {
        public static MbMessageContext Current => GetData();

        private static MbMessageContext GetData()
        {
            MbMessageContext mbMessageContext = new MbMessageContext();

            PipeContext current = MbMessageContextStack.Current;

            ConsumeContext<CorrelatedBy<Guid>> correlationIdContext = current?.GetPayload<ConsumeContext<CorrelatedBy<Guid>>>();

            if (correlationIdContext != null)
            {
                mbMessageContext.CorrelationId = correlationIdContext.Message.CorrelationId;
            }

            ConsumeContext<ITenantIdMbMessage> tenantIdContext = current?.GetPayload<ConsumeContext<ITenantIdMbMessage>>();

            if (tenantIdContext != null)
            {
                mbMessageContext.TenantId = tenantIdContext.Message.TenantId;
            }

            if (mbMessageContext.CorrelationId != Guid.Empty || mbMessageContext.TenantId != Guid.Empty)
            {
                return mbMessageContext;
            }
            else
            {
                return null;
            }
        }
    }
}
