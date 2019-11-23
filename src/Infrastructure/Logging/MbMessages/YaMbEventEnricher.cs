using MbEvents;
using Serilog.Core;
using Serilog.Events;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    /// <summary>
    /// Enriches Serilog data with custom message bus event data.
    /// </summary>
    public class YaMbEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            ITenantIdMbMessage mbEvent = YaMbEventEnricherData.Current;

            if (mbEvent != null)
            {
                logEvent.AddPropertyIfAbsent(factory.CreateProperty(nameof(mbEvent.TenantId), mbEvent.TenantId.ToString(), true));
            }
        }
    }
}
