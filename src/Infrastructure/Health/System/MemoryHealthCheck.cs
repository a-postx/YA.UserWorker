using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace YA.TenantWorker.Infrastructure.Health.System
{
    /// <summary>
    /// Checks memory-related values of the application.
    /// </summary>
    public class MemoryHealthCheck : IHealthCheck
    {
        public MemoryHealthCheck(IOptionsMonitor<MemoryCheckOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private readonly IOptionsMonitor<MemoryCheckOptions> _options;

        public string Name => "memory_check";

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            MemoryCheckOptions opts = _options.Get(context.Registration.Name);

            double workingSetMb = Process.GetCurrentProcess().WorkingSet64 / 1000000d;
            double privateMemoryMb = Process.GetCurrentProcess().PrivateMemorySize64 / 1000000d;
            double managedMemoryMb = GC.GetTotalMemory(forceFullCollection: false) / 1000000d;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "MemoryWorkingSetMBytes", workingSetMb },
                { "MemoryPrivateMBytes", privateMemoryMb },
                { "MemoryManagedMBytes", managedMemoryMb },
                { "MemoryGen0Collections", GC.CollectionCount(0) },
                { "MemoryGen1Collections", GC.CollectionCount(1) },
                { "MemoryGen2Collections", GC.CollectionCount(2) }
            };

            HealthStatus status = managedMemoryMb < opts.ProcessMaxMemoryThreshold
                ? HealthStatus.Healthy
                : HealthStatus.Unhealthy;

            return Task.FromResult(new HealthCheckResult(status,
                $"Reports degraded status if managed memory size >= {opts.ProcessMaxMemoryThreshold} megabytes.", null, data));
        }
    }
}
