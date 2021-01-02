using GreenPipes;
using MassTransit;
using MbEvents;
using Serilog.Core;
using Serilog.Events;
using YA.TenantWorker.Infrastructure.Messaging.Filters;

namespace YA.TenantWorker.Infrastructure.Logging.Enrichers
{
    /// <summary>
    /// Enriches Serilog data with custom context data from bus message.
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
}
