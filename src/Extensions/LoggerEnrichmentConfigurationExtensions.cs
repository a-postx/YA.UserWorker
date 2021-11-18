using Serilog;
using Serilog.Configuration;
using YA.UserWorker.Infrastructure.Logging.Enrichers;

namespace YA.UserWorker.Extensions;

/// <summary>
/// Provides various extension methods for configuring Serilog.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enriches the Serilog logging data with custom message bus event context information.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static LoggerConfiguration FromCustomMbMessageContext(this LoggerEnrichmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.With<YaCustomMbEventEnricher>();
    }
}
