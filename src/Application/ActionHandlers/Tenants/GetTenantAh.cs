using System.Globalization;
using Delobytes.AspNetCore.Application;
using Delobytes.Mapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.Tenants.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class GetTenantAh : IGetTenantAh
{
    public GetTenantAh(ILogger<GetTenantAh> logger,
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

    private readonly ILogger<GetTenantAh> _log;
    private readonly IActionContextAccessor _actionCtx;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IMediator _mediator;
    private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

    public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        Guid tenantId = _runtimeCtx.GetTenantId();

        ICommandResult<Tenant> result = await _mediator
        .Send(new GetTenantCommand(tenantId), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                if (_actionCtx.ActionContext.HttpContext
                    .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                _actionCtx.ActionContext.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                TenantVm tenantViewModel = _tenantVmMapper.Map(result.Data);

                return new OkObjectResult(tenantViewModel);
        }
    }
}
