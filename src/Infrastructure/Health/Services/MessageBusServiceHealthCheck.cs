using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Infrastructure.Messaging.Test;

namespace YA.UserWorker.Infrastructure.Health.Services
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
            Response<IUserWorkerTestResponseV1> response = null;
            Dictionary<string, object> healthData = new Dictionary<string, object>();

            DateTime startDt = DateTime.UtcNow;

            try
            {
                response = await _bus.Request<IUserWorkerTestRequestV1, IUserWorkerTestResponseV1>(new {TimeStamp = now}, cancellationToken);
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
                DateTime stopDt = DateTime.UtcNow;
                TimeSpan processingTime = stopDt - startDt;
                healthData.Add("MessageBusDelayMsec", (int)processingTime.TotalMilliseconds);
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
