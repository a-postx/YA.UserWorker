using Serilog;
using Serilog.Configuration;
using YA.UserWorker.Infrastructure.Logging.Enrichers;

namespace YA.UserWorker.Extensions;

public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Обогащает логи Серилога данными из сообщений шины.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static LoggerConfiguration FromCustomMbMessageContext(this LoggerEnrichmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.With<YaCustomMbEventEnricher>();
    }
}
