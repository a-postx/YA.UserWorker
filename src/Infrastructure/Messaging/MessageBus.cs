using MassTransit;
using MbCommands;
using MbEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Infrastructure.Messaging.Messages;

namespace YA.TenantWorker.Infrastructure.Messaging
{
    public class MessageBus : IMessageBus
    {
        public MessageBus(ILogger<MessageBus> logger,
            IBus bus,
            IPublishEndpoint publishEndpoint)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        private readonly ILogger<MessageBus> _log;
        private readonly IBus _bus;
        private readonly IPublishEndpoint _publishEndpoint;

        public async Task CreateTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ICreateTenantV1>(new CreateTenantV1(correlationId, tenantId, tenantSm), cancellationToken);
        }

        public async Task DeleteTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<IDeleteTenantV1>(new DeleteTenantV1(correlationId, tenantId, tenantSm), cancellationToken);
        }

        public async Task UpdateTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<IUpdateTenantV1>(new UpdateTenantV1(correlationId, tenantId, tenantSm), cancellationToken);
        }

        public async Task SendPricingTierV1Async(Guid correlationId, Guid tenantId, PricingTierTm pricingTierTm, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ISendPricingTierV1>(new SendPricingTierV1(correlationId, tenantId, pricingTierTm), cancellationToken);
        }
    }
}
