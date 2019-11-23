using System;
using MassTransit;
using YA.TenantWorker.Application.Models.SaveModels;

namespace MbEvents
{
    public interface ITenantIdMbMessage
    {
        Guid TenantId { get; }
    }

    public interface ICreateTenantV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }

    public interface IDeleteTenantV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }

    public interface IUpdateTenantV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantSm Tenant { get; }
    }
}
