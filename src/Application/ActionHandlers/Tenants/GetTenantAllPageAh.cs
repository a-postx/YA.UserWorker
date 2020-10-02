using Delobytes.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.Common.Constants;
using YA.TenantWorker.Application.Features.Tenants.Queries;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;
using Delobytes.Mapper;
using System.Collections.Generic;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class GetTenantAllPageAh : IGetTenantAllPageAh
    {
        public GetTenantAllPageAh(ILogger<GetTenantAllPageAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            LinkGenerator linkGenerator,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<GetTenantAllPageAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            DateTimeOffset? createdAfter = Cursor.FromCursor<DateTimeOffset?>(pageOptions.Before);
            DateTimeOffset? createdBefore = Cursor.FromCursor<DateTimeOffset?>(pageOptions.After);

            ICommandResult<PaginatedResult<Tenant>> result = await _mediator
                .Send(new GetTenantAllPageCommand(pageOptions, createdAfter, createdBefore), cancellationToken);

            switch (result.Status)
            {
                case CommandStatuses.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatuses.BadRequest:
                    return new BadRequestResult();
                case CommandStatuses.NotFound:
                    return new NotFoundResult();
                case CommandStatuses.Ok:
                    PaginatedResult<Tenant> resultBm = result.Data;

                    (string startCursor, string endCursor) = Cursor.GetFirstAndLastCursor(resultBm.Items, x => x.CreatedDateTime);

                    List<TenantVm> itemVms = _tenantVmMapper.MapList(resultBm.Items);

                    PaginatedResultVm<TenantVm> paginatedResultVm = new PaginatedResultVm<TenantVm>(
                        _linkGenerator,
                        pageOptions,
                        resultBm.HasNextPage,
                        resultBm.HasPreviousPage,
                        resultBm.TotalCount,
                        startCursor,
                        endCursor,
                        _actionCtx.ActionContext.HttpContext,
                        RouteNames.GetTenantPage,
                        itemVms);

                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(YaHeaderKeys.Link, paginatedResultVm.PageInfo.ToLinkHttpHeaderValue());

                    return new OkObjectResult(paginatedResultVm);
            }
        }
    }
}
