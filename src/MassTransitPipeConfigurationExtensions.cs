using GreenPipes;
using System;
using YA.TenantWorker.Infrastructure.Logging.MbMessages;

namespace YA.TenantWorker
{
    public static class MassTransitPipeConfiguratorExtensions
    {
        /// <summary>
        /// Injects the required configuration into Mass Transit to allow Serilog to acquire custom event's enrichment data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        public static void UseSerilogCustomMbEventEnricher<T>(this IPipeConfigurator<T> configurator) where T : class, PipeContext
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.AddPipeSpecification(new YaMbEventSerilogEnricherSpecification<T>());
        }
    }
}
