using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Audit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YA.TenantWorker.MessageBus
{
    public class MBService : IHostedService
    {
        public MBService(ILogger<MBService> logger, IBusControl busControl, IMessageAuditStore auditStore)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
            _auditStore = auditStore ?? throw new ArgumentNullException(nameof(auditStore));
        }

        private readonly ILogger<MBService> _log;
        private readonly IBusControl _busControl;
        private readonly IMessageAuditStore _auditStore;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Message Bus service is starting.");

            _busControl.ConnectSendAuditObservers(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));
            _busControl.ConnectConsumeAuditObserver(_auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));

            return _busControl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Message Bus service is stopping.");

            return _busControl.StopAsync(cancellationToken);
        }
    }
}
