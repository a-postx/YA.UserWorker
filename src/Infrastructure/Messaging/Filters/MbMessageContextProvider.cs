using GreenPipes;
using MassTransit;
using MbEvents;

namespace YA.UserWorker.Infrastructure.Messaging.Filters;

/// <summary>
/// Добавляет контекст корелляции и арендатора из сообщения брокера.
/// </summary>
internal static class MbMessageContextProvider
{
    public static MbMessageContext Current => GetData();

    private static MbMessageContext GetData()
    {
        MbMessageContext mbMessageContext = new MbMessageContext();

        PipeContext current = MbMessageContextStackWrapper.Current;

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
