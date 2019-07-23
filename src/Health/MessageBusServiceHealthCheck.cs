using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;
using YA.TenantWorker.MessageBus;

namespace YA.TenantWorker.Health
{
    /// <summary>
    /// Regular check (30 sec) for availability of the message bus services.
    /// </summary>
    public class MessageBusServiceHealthCheck : IHealthCheck
    {
        public MessageBusServiceHealthCheck(ILogger<MessageBusServiceHealthCheck> logger, IBusControl bus, IConfiguration config)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private readonly ILogger<MessageBusServiceHealthCheck> _log;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;

        public string Name => General.MessageBusServiceHealthCheckName;

        public bool MessageBusStartupTaskCompleted { get; set; }
        public AutoResetEvent MessageBusCheckCompleted { get; set; }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            DateTime now = DateTime.UtcNow;
            ITenantManagerTestResponseV1 response = null;
            Dictionary<string, object> healthData = new Dictionary<string, object>();
            KeyVaultSecrets secrets = _config.Get<KeyVaultSecrets>();

            try
            {
                Stopwatch mbSW = new Stopwatch();
                mbSW.Start();

                Uri serviceAddress = new Uri($"rabbitmq://{secrets.MessageBusHost}/{secrets.MessageBusVHost}/{MbQueueNames.PrivateServiceQueueName}");

                IRequestClient<ITenantManagerTestRequestV1, ITenantManagerTestResponseV1> client = _bus.CreateRequestClient<ITenantManagerTestRequestV1, ITenantManagerTestResponseV1>(serviceAddress, TimeSpan.FromSeconds(10));
                response = await client.Request(new { TimeStamp = now }, cancellationToken);

                mbSW.Stop();
                
                healthData.Add("Delay, msec", mbSW.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _log.LogError("Error checking health for Message Bus: {Message}", e.Message);
                healthData.Add("Exception", e.Message);
            }

            if (MessageBusStartupTaskCompleted && response?.GotIt == now)
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
