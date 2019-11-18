using Delobytes.AspNetCore;
using Delobytes.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class PostTenantCommand : IPostTenantCommand
    {
        public PostTenantCommand(ILogger<PostTenantCommand> logger,
            IActionContextAccessor actionContextAccessor,
            ITenantWorkerDbContext workerDbContext,
            IMessageBus messageBus,
            IMapper<Tenant, TenantVm> tenantVmMapper,
            IMapper<TenantSm, Tenant> tenantSmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _tenantSmMapper = tenantSmMapper ?? throw new ArgumentNullException(nameof(tenantSmMapper));
        }

        private readonly ILogger<PostTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IMapper<TenantSm, Tenant> _tenantSmMapper;

        public async Task<IActionResult> ExecuteAsync(TenantSm tenantSm, CancellationToken cancellationToken)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationId(General.CorrelationIdHeader);
            
            if (tenantSm.TenantId == Guid.Empty || string.IsNullOrEmpty(tenantSm.TenantName))
            {
                return new BadRequestResult();
            }
            
            using (_log.BeginScopeWith((Logs.TenantId, tenantSm.TenantId)))
            {
                Tenant tenant = _tenantSmMapper.Map(tenantSm);
                tenant.TenantType = TenantTypes.Custom;
                TenantVm tenantVm = _tenantVmMapper.Map(tenant);

                await _dbContext.CreateAndReturnEntityAsync(tenant, cancellationToken);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.CreateTenantV1(tenantSm, correlationId, cancellationToken);

                return new CreatedAtRouteResult(RouteNames.GetTenant, new { TenantId = tenantVm.TenantId, TenantName = tenantVm.TenantName }, tenantVm);
            }
        }
    }
}
