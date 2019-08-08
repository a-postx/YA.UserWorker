using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Messaging.Test;

namespace YA.TenantWorker.Health.Services
{
    /// <summary>
    /// Regular check (30 sec) for availability of the message bus services.
    /// </summary>
    public class MessageBusServiceHealthCheck : IHealthCheck
    {
        public MessageBusServiceHealthCheck(ILogger<MessageBusServiceHealthCheck> logger, IBus bus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        private readonly ILogger<MessageBusServiceHealthCheck> _log;
        private readonly IBus _bus;

        public string Name => General.MessageBusServiceHealthCheckName;

        public bool MessageBusStartupTaskCompleted { get; set; }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            DateTime now = DateTime.UtcNow;
            Response<ITenantWorkerTestResponseV1> response = null;
            Dictionary<string, object> healthData = new Dictionary<string, object>();

            try
            {
                Stopwatch mbSW = new Stopwatch();
                mbSW.Start();

                response = await _bus.Request<ITenantWorkerTestRequestV1, ITenantWorkerTestResponseV1>(new { TimeStamp = now }, cancellationToken);

                mbSW.Stop();
                
                healthData.Add("Delay, msec", mbSW.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _log.LogError("Error checking health for Message Bus: {Message}", e);
                healthData.Add("Exception", e.Message);
            }

            if (MessageBusStartupTaskCompleted && response?.Message?.GotIt == now)
            {
                return HealthCheckResult.Healthy("Message Bus is available.", healthData);
            }
            else
            {
                return HealthCheckResult.Unhealthy("Message Bus is unavailable.", null, healthData);
            }
        }
    }
}
