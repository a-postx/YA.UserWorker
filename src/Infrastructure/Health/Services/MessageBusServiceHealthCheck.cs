using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Infrastructure.Messaging.Test;

namespace YA.TenantWorker.Infrastructure.Health.Services
{
    /// <summary>
    /// Regular check for availability of the message bus services.
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

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            DateTime now = DateTime.UtcNow;
            Response<ITenantWorkerTestResponseV1> response = null;
            Dictionary<string, object> healthData = new Dictionary<string, object>();

            Stopwatch mbSw = new Stopwatch();
            mbSw.Start();

            try
            {
                response = await _bus.Request<ITenantWorkerTestRequestV1, ITenantWorkerTestResponseV1>(new {TimeStamp = now}, cancellationToken);
            }
            catch (RequestException ex)
            {
                healthData.Add("MessageBusHealthCheckException", ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error checking health for Message Bus");
                healthData.Add("MessageBusHealthCheckException", ex.Message);
            }
            finally
            {
                mbSw.Stop();
                healthData.Add("MessageBusDelayMsec", mbSw.ElapsedMilliseconds);
            }

            if (response?.Message?.GotIt == now)
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
