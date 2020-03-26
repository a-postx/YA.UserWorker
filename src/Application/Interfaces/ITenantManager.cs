using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface ITenantManager
    {
        Task<PricingTierTm> GetPricingTierMbTmAsync(CancellationToken cancellationToken);
    }
}
