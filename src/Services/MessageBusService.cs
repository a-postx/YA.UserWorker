using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health.Services;
using YA.TenantWorker.Messaging.Test;

namespace YA.TenantWorker.Services
{
    /// <summary>
    /// Hosted service for Message Bus
    /// </summary>
    public class MessageBusService : IHostedService
    {
        public MessageBusService(ILogger<MessageBusService> logger,
            IConfiguration config,
            IBusControl busControl,
            IMessageAuditStore auditStore,
            MessageBusServiceHealthCheck messageBusServiceHealthCheck)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
            _auditStore = auditStore ?? throw new ArgumentNullException(nameof(auditStore));
            _messageBusServiceHealthCheck = messageBusServiceHealthCheck ?? throw new ArgumentNullException(nameof(messageBusServiceHealthCheck));
        }

        private readonly ILogger<MessageBusService> _log;
        private readonly IConfiguration _config;
        private readonly IBusControl _busControl;
        private readonly IMessageAuditStore _auditStore;
        private readonly MessageBusServiceHealthCheck _messageBusServiceHealthCheck;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " startup service is starting.");

            KeyVaultSecrets secrets = _config.Get<KeyVaultSecrets>();

            bool success = false;

            while (!success)
            {
                bool result = false;

                try
                {
                    result = await Utils.CheckTcpConnectionAsync(secrets.MessageBusHost, General.MessageBusServiceHealthPort);
                }
                catch (Exception e)
                {
                    _log.LogError(nameof(MessageBusService) + " startup service has failed: {Exception}", e);
                }

                if (result)
                {
                    success = true;
                    _messageBusServiceHealthCheck.MessageBusStartupTaskCompleted = true;

                    _log.LogInformation(nameof(MessageBusService) + " startup service has started.");
                }
                else
                {
                    await Task.Delay(General.StartupServiceCheckRetryIntervalMs);
                }
            }

            _busControl.ConnectSendAuditObservers(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));
            _busControl.ConnectConsumeAuditObserver(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));

            await _busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " startup service is stopping.");

            await _busControl.StopAsync(cancellationToken);
        }
    }
}
