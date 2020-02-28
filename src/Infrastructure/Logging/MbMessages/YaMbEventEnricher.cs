using GreenPipes;
using MassTransit;
using MbEvents;
using Serilog.Core;
using Serilog.Events;
using YA.TenantWorker.Infrastructure.Messaging.Filters;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    /// <summary>
    /// Enriches Serilog data with custom context data from bus message.
    /// </summary>
    public class YaMbEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            PipeContext current = MbMessageContextStack.Current;
            ConsumeContext<ITenantIdMbMessage> context = current?.GetPayload<ConsumeContext<ITenantIdMbMessage>>();

            if (context != null)
            {
                logEvent.AddPropertyIfAbsent(factory
                    .CreateProperty(nameof(context.Message.TenantId), context.Message.TenantId.ToString(), true));
            }
        }
    }
}
