using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface ITenantManager
    {
        Task<PricingTierTm> GetPricingTierMbTransferModelAsync(Guid correlationId, Guid tenantId, CancellationToken cancellationToken);
    }
}
