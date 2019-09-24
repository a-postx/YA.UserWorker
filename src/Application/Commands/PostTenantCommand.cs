using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Delobytes.Mapper;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
            _tenantWorkerDbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _tenantSmMapper = tenantSmMapper ?? throw new ArgumentNullException(nameof(tenantSmMapper));
        }

        private readonly ILogger<PostTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _tenantWorkerDbContext;
        private readonly IMessageBus _messageBus;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IMapper<TenantSm, Tenant> _tenantSmMapper;

        public async Task<IActionResult> ExecuteAsync(TenantSm tenantSm, CancellationToken cancellationToken)
        {
            if (tenantSm.TenantName == string.Empty)
            {
                return new BadRequestResult();
            }
            
            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromContext();

            using (_log.BeginScopeWith((Logs.TenantId, tenantSm.TenantId), (Logs.CorrelationId, correlationId)))
            {
                Tenant tenant = _tenantSmMapper.Map(tenantSm);
                tenant.CorrelationId = correlationId;
                tenant.TenantType = TenantTypes.Custom;

                try
                {
                    await _tenantWorkerDbContext.CreateEntityAsync(tenant, cancellationToken);
                    await _tenantWorkerDbContext.ApplyChangesAsync(cancellationToken);

                    await _messageBus.CreateTenantV1(tenantSm, correlationId, cancellationToken);
                }
                catch (Exception e) when (_log.LogException(e))
                {
                    throw;
                }

                TenantVm tenantVm = _tenantVmMapper.Map(tenant);

                return new CreatedAtRouteResult(RouteNames.GetTenant, new { TenantId = tenantVm.TenantId, TenantName = tenantVm.TenantName }, tenantVm);
            }
        }
    }
}
