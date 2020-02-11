using Delobytes.Mapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application
{
    public class TenantManager : ITenantManager
    {
        public TenantManager(
            ILogger<TenantManager> logger,
            ITenantWorkerDbContext dbContext,
            IMessageBus messageBus,
            IMapper<PricingTier, PricingTierTm> pricingTierToTmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _pricingTierToTmMapper = pricingTierToTmMapper ?? throw new ArgumentNullException(nameof(pricingTierToTmMapper));
        }

        private readonly ILogger<TenantManager> _log;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        private readonly IMapper<PricingTier, PricingTierTm> _pricingTierToTmMapper;

        public async Task<PricingTierTm> GetPricingTierMbTransferModelAsync(Guid correlationId, Guid tenantId, CancellationToken cancellationToken)
        {
            Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(e => e.TenantID == tenantId, cancellationToken);

            PricingTierTm pricingTierTm = new PricingTierTm();
            _pricingTierToTmMapper.Map(tenant.PricingTier, pricingTierTm);

            return pricingTierTm;
        }
    }
}
