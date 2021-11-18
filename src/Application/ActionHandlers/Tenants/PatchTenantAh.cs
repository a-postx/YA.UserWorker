using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Actions;
using Delobytes.Mapper;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class PatchTenantAh : IPatchTenantAh
{
    public PatchTenantAh(ILogger<PatchTenantAh> logger,
        IActionContextAccessor actionCtx,
        IRuntimeContextAccessor runtimeContext,
        IMediator mediator,
        IMapper<Tenant, TenantVm> tenantVmMapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
    }

    private readonly ILogger<PatchTenantAh> _log;
    private readonly IActionContextAccessor _actionCtx;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IMediator _mediator;
    private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

    public async Task<IActionResult> ExecuteAsync(JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
    {
        Guid tenantId = _runtimeCtx.GetTenantId();

        ICommandResult<Tenant> result = await _mediator
            .Send(new UpdateTenantCommand(tenantId, patch), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.ModelInvalid:
                return new BadRequestObjectResult(new Failure(_runtimeCtx.GetCorrelationId(), result.ErrorMessages));
            case CommandStatus.BadRequest:
                return new BadRequestResult();
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                TenantVm tenantVm = _tenantVmMapper.Map(result.Data);
                //return new OkObjectResult(new Success<TenantVm>(_runtimeCtx.GetCorrelationId(), result.Data.tstamp.ToString(), tenantVm));
                return new OkObjectResult(tenantVm);
        }
    }
}
