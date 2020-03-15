using AutoMapper;
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
        public TenantManager(ILogger<TenantManager> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IMapper mapper,
            ITenantWorkerDbContext dbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeContext = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<TenantManager> _log;
        private readonly IRuntimeContextAccessor _runtimeContext;
        private readonly IMapper _mapper;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<PricingTierTm> GetPricingTierMbTransferModelAsync(CancellationToken cancellationToken)
        {
            Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

            PricingTierTm pricingTierTm = _mapper.Map<PricingTierTm>(tenant.PricingTier);

            return pricingTierTm;
        }
    }
}
