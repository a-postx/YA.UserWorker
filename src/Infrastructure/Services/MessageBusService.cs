using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Audit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YA.Common;
using YA.TenantWorker.Health.Services;
using YA.TenantWorker.Infrastructure.Messaging.Test;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Infrastructure.Services
{
    /// <summary>
    /// Hosted service for Message Bus
    /// </summary>
    public class MessageBusService : BackgroundService
    {
        public MessageBusService(ILogger<MessageBusService> logger,
            IOptions<AppSecrets> secrets,
            IBusControl busControl,
            IMessageAuditStore auditStore,
            MessageBusServiceHealthCheck messageBusServiceHealthCheck)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSecrets = secrets.Value;
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
            _auditStore = auditStore ?? throw new ArgumentNullException(nameof(auditStore));
            _messageBusServiceHealthCheck = messageBusServiceHealthCheck ?? throw new ArgumentNullException(nameof(messageBusServiceHealthCheck));
        }

        private readonly ILogger<MessageBusService> _log;
        private readonly AppSecrets _appSecrets;
        private readonly IBusControl _busControl;
        private readonly IMessageAuditStore _auditStore;
        private readonly MessageBusServiceHealthCheck _messageBusServiceHealthCheck;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " background service is starting...");

            bool success = false;

            while (!success)
            {
                try
                {
                    success = await NetTools.CheckTcpConnectionAsync(_appSecrets.MessageBusHost, _appSecrets.MessageBusPort);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, nameof(MessageBusService) + " background service check has failed");
                }

                if (success)
                {
                    _messageBusServiceHealthCheck.MessageBusStartupTaskCompleted = true;

                    _log.LogInformation(nameof(MessageBusService) + " background service check succeeded.");
                }
                else
                {
                    await Task.Delay(10000, cancellationToken);
                }
            }

            _busControl.ConnectSendAuditObservers(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));
            _busControl.ConnectConsumeAuditObserver(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));

            await _busControl.StartAsync(cancellationToken);

            _log.LogInformation(nameof(MessageBusService) + " background service has started.");
        }

        //не выполняется, сервис находится как IHostedService
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " background service is stopping...");

            await _busControl.StopAsync(cancellationToken);

            _log.LogInformation(nameof(MessageBusService) + " background service gracefully stopped.");
        }
    }
}
