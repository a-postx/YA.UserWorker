using AutoMapper;
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
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class PatchTenantByIdCommand : IPatchTenantByIdCommand
    {
        public PatchTenantByIdCommand(
            ILogger<PatchTenantByIdCommand> logger,
            IMapper mapper,
            IActionContextAccessor actionContextAccessor,
            IObjectModelValidator objectModelValidator,
            ITenantWorkerDbContext dbContext,
            IMessageBus messageBus,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _objectModelValidator = objectModelValidator ?? throw new ArgumentNullException(nameof(objectModelValidator));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<PatchTenantByIdCommand> _log;
        private readonly IMapper _mapper;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;
        private readonly IObjectModelValidator _objectModelValidator;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(Guid tenantId, JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
        {
            if (tenantId == Guid.Empty || patch == null)
            {
                return new BadRequestResult();
            }

            Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

            if (tenant == null)
            {
                return new NotFoundResult();
            }

            TenantSm tenantSm = _mapper.Map<TenantSm>(tenant);

            ModelStateDictionary modelState = _actionContextAccessor.ActionContext.ModelState;
            patch.ApplyTo(tenantSm, modelState);

            _objectModelValidator.Validate(_actionContextAccessor.ActionContext, null, null, tenantSm);

            if (!modelState.IsValid)
            {
                return new BadRequestObjectResult(modelState);
            }

            tenant = (Tenant)_mapper.Map(tenantSm, tenant, typeof(TenantSm), typeof(Tenant));

            _dbContext.UpdateTenant(tenant);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.TenantUpdatedV1Async(tenantId, _mapper.Map<TenantTm>(tenant), cancellationToken);

            TenantVm tenantVm = _tenantVmMapper.Map(tenant);

            return new OkObjectResult(tenantVm);
        }
    }
}
