using AutoMapper;
using Delobytes.AspNetCore;
using Delobytes.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class PostTenantCommand : IPostTenantCommand
    {
        public PostTenantCommand(ILogger<PostTenantCommand> logger,
            IMapper mapper,
            IActionContextAccessor actionContextAccessor,
            ITenantWorkerDbContext workerDbContext,
            IMessageBus messageBus,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<PostTenantCommand> _log;
        private readonly IMapper _mapper;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(TenantSm tenantSm, CancellationToken cancellationToken)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationId(General.CorrelationIdHeader);
            
            if (tenantSm.TenantId == Guid.Empty || string.IsNullOrEmpty(tenantSm.TenantName))
            {
                return new BadRequestResult();
            }
            
            Tenant tenant = _mapper.Map<Tenant>(tenantSm);
            tenant.TenantType = TenantTypes.Custom;

            Guid defaultPricingTierId = Guid.Parse(SeedEntities.DefaultPricingTierId);

            PricingTier defaultPricingTier = await _dbContext.GetEntityAsync<PricingTier>(e => e.PricingTierID == defaultPricingTierId, cancellationToken);
            tenant.PricingTier = defaultPricingTier;

            TenantVm tenantVm = _tenantVmMapper.Map(tenant);

            await _dbContext.CreateAndReturnEntityAsync(tenant, cancellationToken);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.TenantCreatedV1Async(correlationId, tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

            return new CreatedAtRouteResult(RouteNames.GetTenant, new { TenantId = tenantVm.TenantId, TenantName = tenantVm.TenantName }, tenantVm);
        }
    }
}
