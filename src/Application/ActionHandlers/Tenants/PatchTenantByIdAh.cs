using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Actions;
using Delobytes.Mapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class PatchTenantByIdAh : IPatchTenantByIdAh
{
    public PatchTenantByIdAh(ILogger<PatchTenantAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IRuntimeContextAccessor runtimeContext,
        IMapper<Tenant, TenantVm> tenantVmMapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
    }

    private readonly ILogger<PatchTenantAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;
    private readonly IRuntimeContextAccessor _runtimeCtx;

    private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

    public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
    {
        ICommandResult<Tenant> result = await _mediator
            .Send(new UpdateTenantByIdCommand(yaTenantId, patch), cancellationToken);

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
                return new OkObjectResult(tenantVm);
        }
    }
}
