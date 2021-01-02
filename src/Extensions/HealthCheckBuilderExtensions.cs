using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using YA.TenantWorker.Infrastructure.Health.System;

namespace YA.TenantWorker.Extensions
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddMemoryHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            int? thresholdInMBytes = null)
        {
            builder.AddCheck<MemoryHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            if (thresholdInMBytes.HasValue)
            {
                builder.Services.Configure<MemoryCheckOptions>(name, options =>
                {
                    options.ProcessMaxMemoryThreshold = thresholdInMBytes.Value;
                });
            }

            return builder;
        }

        public static IHealthChecksBuilder AddNetworkHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            int? latencyInMseconds = null)
        {
            builder.AddCheck<NetworkHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            if (latencyInMseconds.HasValue)
            {
                builder.Services.Configure<NetworkCheckOptions>(name, options =>
                {
                    options.MaxLatencyThreshold = latencyInMseconds.Value;
                });
            }

            return builder;
        }

        public static IHealthChecksBuilder AddGenericHealthCheck<T>(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null) where T : class, IHealthCheck
        {
            builder.AddCheck<T>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }
    }
}
