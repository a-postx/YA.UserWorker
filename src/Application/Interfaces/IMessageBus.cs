using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IMessageBus
    {
        Task TenantCreatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
        Task TenantDeletedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
        Task TenantUpdatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);

        Task SendPricingTierV1Async(PricingTierTm pricingTierTm, CancellationToken cancellationToken);
    }
}
