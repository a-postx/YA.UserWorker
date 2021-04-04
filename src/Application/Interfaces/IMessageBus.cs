using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Application.Models.Dto;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IMessageBus
    {
        Task TenantCreatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
        Task TenantDeletedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
        Task TenantUpdatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
    }
}
