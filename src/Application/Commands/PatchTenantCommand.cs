using Delobytes.Mapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Dto.SaveModels;
using YA.TenantWorker.Application.Dto.ViewModels;

namespace YA.TenantWorker.Application.Commands
{
    public class PatchTenantCommand : IPatchTenantCommand
    {
        public PatchTenantCommand(
            ILogger<PatchTenantCommand> logger,
            IActionContextAccessor actionContextAccessor,
            IObjectModelValidator objectModelValidator,
            ITenantWorkerDbContext workerDbContext,
            IMessageBus messageBus,
            IMapper<Tenant, TenantVm> tenantVmMapper,
            IMapper<Tenant, TenantSm> tenantSmMapper,
            IMapper<TenantSm, Tenant> tenantMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _objectModelValidator = objectModelValidator ?? throw new ArgumentNullException(nameof(objectModelValidator));
            _tenantWorkerDbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _tenantSmMapper = tenantSmMapper ?? throw new ArgumentNullException(nameof(tenantSmMapper));
            _tenantMapper = tenantMapper ?? throw new ArgumentNullException(nameof(tenantMapper));
        }

        private readonly ILogger<PatchTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _tenantWorkerDbContext;
        private readonly IMessageBus _messageBus;
        private readonly IObjectModelValidator _objectModelValidator;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IMapper<Tenant, TenantSm> _tenantSmMapper;
        private readonly IMapper<TenantSm, Tenant> _tenantMapper;

        public async Task<IActionResult> ExecuteAsync(Guid tenantId, JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
        {
            if (tenantId == Guid.Empty || patch == null)
            {
                return new BadRequestResult();
            }

            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromContext();

            using (_log.BeginScopeWith((Logs.TenantId, tenantId), (Logs.CorrelationId, correlationId)))
            {
                Tenant tenant = await _tenantWorkerDbContext.GetEntityAsync<Tenant>(e => e.TenantID == tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new NotFoundResult();
                }

                tenant.CorrelationId = correlationId;

                TenantSm tenantSm = _tenantSmMapper.Map(tenant);

                ModelStateDictionary modelState = _actionContextAccessor.ActionContext.ModelState;
                patch.ApplyTo(tenantSm, modelState);

                _objectModelValidator.Validate(_actionContextAccessor.ActionContext, null, null, tenantSm);

                if (!modelState.IsValid)
                {
                    return new BadRequestObjectResult(modelState);
                }

                _tenantMapper.Map(tenantSm, tenant);

                try
                {
                    _tenantWorkerDbContext.UpdateTenant(tenant);
                    await _tenantWorkerDbContext.ApplyChangesAsync(cancellationToken);

                    await _messageBus.UpdateTenantV1(tenantSm, correlationId, cancellationToken);
                }
                catch (Exception e) when (_log.LogException(e))
                {
                    throw;
                }

                TenantVm tenantVm = _tenantVmMapper.Map(tenant);

                return new OkObjectResult(tenantVm);
            }
        }
    }
}
