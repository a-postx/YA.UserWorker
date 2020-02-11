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
        public GetPricingTierConsumer(ILogger<GetPricingTierConsumer> logger, IMessageBus messageBus, ITenantManager tenantManager)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
        }

        private readonly ILogger<GetPricingTierConsumer> _log;
        private readonly IMessageBus _messageBus;
        private readonly ITenantManager _tenantManager;

        public async Task Consume(ConsumeContext<IGetPricingTierV1> context)
        {
            try
            {
                PricingTierTm pricingTierTm = await _tenantManager.GetPricingTierMbTransferModelAsync(context.Message.CorrelationId,
                    context.Message.TenantId, context.CancellationToken);

                await context.RespondAsync<ISendPricingTierV1>(new SendPricingTierV1(context.Message.CorrelationId, context.Message.TenantId, pricingTierTm));

                //await _messageBus.SendPricingTierV1Async(context.Message.CorrelationId,
                //    context.Message.TenantId, pricingTierTm, context.CancellationToken);
            }
            catch (Exception e) when (_log.LogException(e, (Logs.MbMessage, context.Message.ToJson())))
            {
                throw;
            }
        }
    }
}
