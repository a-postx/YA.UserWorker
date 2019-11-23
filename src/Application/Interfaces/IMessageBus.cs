using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IMessageBus
    {
        Task CreateTenantV1(Guid tenantId, Guid correlationId, TenantSm tenantSm, CancellationToken cancellationToken);
        Task DeleteTenantV1(Guid tenantId, Guid correlationId, TenantSm tenantSm, CancellationToken cancellationToken);
        Task UpdateTenantV1(Guid tenantId, Guid correlationId, TenantSm tenantSm, CancellationToken cancellationToken);
    }
}
