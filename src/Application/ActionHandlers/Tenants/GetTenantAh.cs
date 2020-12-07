using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Features.Tenants.Queries;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;
using Delobytes.Mapper;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class GetTenantAh : IGetTenantAh
    {
        public GetTenantAh(ILogger<GetTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<GetTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            ICommandResult<Tenant> result = await _mediator
                .Send(new GetTenantCommand(), cancellationToken);

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
}
