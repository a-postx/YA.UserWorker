using MbCommands;
using System;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Infrastructure.Messaging.Messages
{
    internal class SendPricingTierV1 : ISendPricingTierV1
    {
        internal SendPricingTierV1(Guid correlationId, Guid tenantId, PricingTierTm pricingTierTm)
        {
            CorrelationId = correlationId;
            TenantId = tenantId;
            PricingTier = pricingTierTm;
        }

        public Guid CorrelationId { get; private set; }
        public Guid TenantId { get; private set; }
        public PricingTierTm PricingTier { get; private set; }
    }
}
