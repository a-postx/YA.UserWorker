using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IMessageBus
    {
        Task CreateTenantV1(TenantSm tenantSm, Guid correlationId, CancellationToken cancellationToken);
        Task DeleteTenantV1(Guid tenantId, Guid correlationId, CancellationToken cancellationToken);
        Task UpdateTenantV1(TenantSm tenantSm, Guid correlationId, CancellationToken cancellationToken);
    }
}
