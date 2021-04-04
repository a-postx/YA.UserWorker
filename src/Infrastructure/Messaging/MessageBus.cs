using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MbEvents;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Infrastructure.Messaging.Messages;

namespace YA.UserWorker.Infrastructure.Messaging
{
    public class MessageBus : IMessageBus
    {
        public MessageBus(ILogger<MessageBus> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IBus bus,
            IPublishEndpoint publishEndpoint)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeCtx = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        private readonly ILogger<MessageBus> _log;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IBus _bus;
        private readonly IPublishEndpoint _publishEndpoint;

        public async Task TenantCreatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint
                .Publish<ITenantCreatedV1>(new TenantCreatedV1(_runtimeCtx.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }

        public async Task TenantDeletedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint
                .Publish<ITenantDeletedV1>(new TenantDeletedV1(_runtimeCtx.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }

        public async Task TenantUpdatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint
                .Publish<ITenantUpdatedV1>(new TenantUpdatedV1(_runtimeCtx.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }
    }
}
