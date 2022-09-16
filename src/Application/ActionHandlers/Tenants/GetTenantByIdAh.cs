using System.Globalization;
using Delobytes.AspNetCore.Application;
using Delobytes.Mapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.Tenants.Queries;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class GetTenantByIdAh : IGetTenantByIdAh
{
    public GetTenantByIdAh(ILogger<GetTenantByIdAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IMapper<Tenant, TenantVm> tenantVmMapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
    }

    private readonly ILogger<GetTenantByIdAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;
    private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

    public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, CancellationToken cancellationToken)
    {
        ICommandResult<Tenant> result = await _mediator
            .Send(new GetTenantByIdCommand(yaTenantId), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.BadRequest:
                return new BadRequestResult();
            case CommandStatus.Ok:
                if (_httpCtx.HttpContext
                    .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                _httpCtx.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                TenantVm tenantViewModel = _tenantVmMapper.Map(result.Data);

                return new OkObjectResult(tenantViewModel);
        }
    }
}
