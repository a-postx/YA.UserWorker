using System;
using MassTransit;
using YA.TenantWorker.SaveModels;

namespace MbEvents
{
    public interface ICreateTenantV1 : CorrelatedBy<Guid>
    {
        TenantSm Tenant { get; }
    }

    public interface IDeleteTenantV1 : CorrelatedBy<Guid>
    {
        Guid TenantID { get; }
    }

    public interface IUpdateTenantV1 : CorrelatedBy<Guid>
    {
        TenantSm Tenant { get; }
    }
}
