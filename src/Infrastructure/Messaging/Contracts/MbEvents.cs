using System;
using MassTransit;
using YA.TenantWorker.Application.Models.Dto;

namespace MbEvents
{
    public interface ITenantIdMbMessage
    {
        Guid TenantId { get; }
    }

    public interface ITenantCreatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }

    public interface ITenantDeletedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }

    public interface ITenantUpdatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }
}
