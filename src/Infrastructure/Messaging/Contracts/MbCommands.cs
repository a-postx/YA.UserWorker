using System;
using MassTransit;
using MbEvents;

namespace MbCommands
{
    public interface IGetPricingTierV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {

    }
}
