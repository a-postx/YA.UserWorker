using MassTransit;
using MbCommands;
using MbEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Infrastructure.Messaging.Messages;

namespace YA.TenantWorker.Infrastructure.Messaging
{
    public class MessageBus : IMessageBus
    {
        public MessageBus(ILogger<MessageBus> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IBus bus,
            IPublishEndpoint publishEndpoint)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeContext = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        private readonly ILogger<MessageBus> _log;
        private readonly IRuntimeContextAccessor _runtimeContext;
        private readonly IBus _bus;
        private readonly IPublishEndpoint _publishEndpoint;

        public async Task TenantCreatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ITenantCreatedV1>(new TenantCreatedV1(_runtimeContext.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }

        public async Task TenantDeletedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ITenantDeletedV1>(new TenantDeletedV1(_runtimeContext.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }

        public async Task TenantUpdatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ITenantUpdatedV1>(new TenantUpdatedV1(_runtimeContext.GetCorrelationId(), tenantId, tenantTm), cancellationToken);
        }

        public async Task SendPricingTierV1Async(PricingTierTm pricingTierTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ISendPricingTierV1>(new SendPricingTierV1(_runtimeContext.GetCorrelationId(), _runtimeContext.GetTenantId(), pricingTierTm), cancellationToken);
        }
    }
}
