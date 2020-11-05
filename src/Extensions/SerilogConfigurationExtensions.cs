using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using System;
using YA.TenantWorker.Infrastructure.Logging.MbMessages;

namespace YA.TenantWorker.Extensions
{
    /// <summary>
    /// Provides various extension methods for configuring Serilog.
    /// </summary>
    public static class SerilogConfigurationExtensions
    {
        /// <summary>
        /// Enriches the Serilog logging data with custom message bus event context information.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static LoggerConfiguration FromCustomMbMessageContext(this LoggerEnrichmentConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.With((ILogEventEnricher)new YaMbEventEnricher());
        }
    }
}
