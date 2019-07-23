using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YA.TenantWorker.Health
{
    /// <summary>
    /// Checks uptime value of the application.
    /// </summary>
    public class UptimeHealthCheck : IHealthCheck
    {
        public string Name => "uptime_check";

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            TimeSpan runtime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            int upTimeValue = (runtime.Days * 3600) + (runtime.Minutes * 60) + runtime.Seconds;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "Uptime, sec", upTimeValue }
            };

            HealthStatus status = HealthStatus.Healthy;

            return Task.FromResult(new HealthCheckResult(status, AppDomain.CurrentDomain.FriendlyName + " is running.", null, data));
        }
    }
}
