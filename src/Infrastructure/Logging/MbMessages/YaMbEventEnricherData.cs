using GreenPipes;
using MassTransit;
using MbEvents;

namespace YA.TenantWorker.Infrastructure.Logging.MbMessages
{
    /// <summary>
    /// Provides enrichment data for custom message bus event.
    /// </summary>
    static class YaMbEventEnricherData
    {
        /// <summary>
        /// Gets the current enrichment data.
        /// </summary>
        public static ITenantIdMbMessage Current => GetData();

        /// <summary>
        /// Gets the enrichment data.
        /// </summary>
        /// <returns></returns>
        static ITenantIdMbMessage GetData()
        {
            PipeContext context = MassTransitPipeContextStack.Current;

            if (context != null)
            {
                if (context.TryGetPayload(out ConsumeContext payload))
                {
                    if (payload.TryGetMessage(out ConsumeContext<ITenantIdMbMessage> message))
                    {
                        if (message.Message is ITenantIdMbMessage mbEvent)
                        {
                            return mbEvent;
                        }
                    }
                }
            }

            return null;
        }
    }
}
