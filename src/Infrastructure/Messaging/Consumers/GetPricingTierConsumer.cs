using MassTransit;
using MbCommands;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.Common.Constants;
using YA.TenantWorker.Application.Features.PricingTiers.Queries;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Extensions;
using YA.TenantWorker.Infrastructure.Messaging.Messages;

namespace YA.TenantWorker.Infrastructure.Messaging.Consumers
{
    public class GetPricingTierConsumer : IConsumer<IGetPricingTierV1>
    {
        public GetPricingTierConsumer(ILogger<GetPricingTierConsumer> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IMessageBus messageBus,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeCtx = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<GetPricingTierConsumer> _log;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IMessageBus _messageBus;
        private readonly IMediator _mediator;

        public async Task Consume(ConsumeContext<IGetPricingTierV1> context)
        {
            try
            {
                ICommandResult<PricingTierTm> result = await _mediator.Send(new GetPricingTierCommand(), context.CancellationToken);

                PricingTierTm pricingTierTm = result.Data;

                await context.RespondAsync<ISendPricingTierV1>(new SendPricingTierV1(_runtimeCtx.GetCorrelationId(), _runtimeCtx.GetTenantId(), pricingTierTm));

                //await _messageBus.PricingTierSentV1Async(_runtimeContext.GetCorrelationId(),
                //    _runtimeCtx.GetTenantId(), pricingTierTm, context.CancellationToken);
            }
            catch (Exception e) when (_log.LogException(e, (YaLogKeys.MbMessage, context.Message.ToJson())))
            {
                throw;
            }
        }
    }
}
