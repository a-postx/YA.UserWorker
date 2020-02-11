using MassTransit;
using MbEvents;
using System;
using YA.TenantWorker.Application.Models.Dto;

namespace MbCommands
{
    public interface IGetPricingTierV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {

    }

    public interface ISendPricingTierV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        PricingTierTm PricingTier { get; }
    }
}
