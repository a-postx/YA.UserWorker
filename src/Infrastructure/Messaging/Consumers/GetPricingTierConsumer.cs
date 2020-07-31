using MassTransit;
using MbCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.Common;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Infrastructure.Messaging.Messages;

namespace YA.TenantWorker.Infrastructure.Messaging.Consumers
{
    public class GetPricingTierConsumer : IConsumer<IGetPricingTierV1>
    {
        public GetPricingTierConsumer(ILogger<GetPricingTierConsumer> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IMessageBus messageBus,
            ITenantManager tenantManager)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeCtx = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
        }

        private readonly ILogger<GetPricingTierConsumer> _log;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IMessageBus _messageBus;
        private readonly ITenantManager _tenantManager;

        public async Task Consume(ConsumeContext<IGetPricingTierV1> context)
        {
            try
            {
                PricingTierTm pricingTierTm = await _tenantManager.GetPricingTierMbTmAsync(context.CancellationToken);

                await context.RespondAsync<ISendPricingTierV1>(new SendPricingTierV1(_runtimeCtx.GetCorrelationId(), _runtimeCtx.GetTenantId(), pricingTierTm));

                //await _messageBus.PricingTierSentV1Async(_runtimeContext.GetCorrelationId(),
                //    _runtimeCtx.GetTenantId(), pricingTierTm, context.CancellationToken);
            }
            catch (Exception e) when (_log.LogException(e, (Logs.MbMessage, context.Message.ToJson())))
            {
                throw;
            }
        }
    }
}
