using GreenPipes;
using MassTransit;
using MbEvents;
using Serilog.Core;
using Serilog.Events;
using YA.UserWorker.Infrastructure.Messaging.Filters;

namespace YA.UserWorker.Infrastructure.Logging.Enrichers;

/// <summary>
/// Обогащает сообщение серилога данными из контекста сообщения брокера.
/// </summary>
public class YaCustomMbEventEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        PipeContext current = MbMessageContextStackWrapper.Current;

        if (current != null)
        {
            if (current.TryGetPayload(out ConsumeContext<ITenantIdMbMessage> tenantContext))
            {
                logEvent.AddPropertyIfAbsent(factory
                    .CreateProperty(nameof(tenantContext.Message.TenantId), tenantContext.Message.TenantId.ToString(), true));
            }
        }
    }
}
