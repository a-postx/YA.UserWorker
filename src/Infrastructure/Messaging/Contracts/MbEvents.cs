using System;
using MassTransit;
using YA.TenantWorker.Application.Models.SaveModels;

namespace MbEvents
{
    public interface ITenantIdMbMessage
    {
        Guid TenantId { get; }
    }

    public interface ITenantCreatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }

    public interface ITenantDeletedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }

    public interface ITenantUpdatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }
}
