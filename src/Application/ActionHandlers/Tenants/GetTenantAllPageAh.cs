using Delobytes.AspNetCore;
using Delobytes.AspNetCore.Application;
using Delobytes.Mapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using YA.Common.Constants;
using YA.UserWorker.Application.Features.Tenants.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.HttpQueryParams;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class GetTenantAllPageAh : IGetTenantAllPageAh
{
    public GetTenantAllPageAh(ILogger<GetTenantAllPageAh> logger,
        IActionContextAccessor actionCtx,
        IMediator mediator,
        IPaginatedResultFactory paginationResultFactory,
        IMapper<Tenant, TenantVm> tenantVmMapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _paginatedResultFactory = paginationResultFactory ?? throw new ArgumentNullException(nameof(paginationResultFactory));
        _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
    }

    private readonly ILogger<GetTenantAllPageAh> _log;
    private readonly IActionContextAccessor _actionCtx;
    private readonly IMediator _mediator;
    private readonly IPaginatedResultFactory _paginatedResultFactory;
    private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

    public async Task<IActionResult> ExecuteAsync(PageOptionsCursor pageOptions, CancellationToken cancellationToken)
    {
        DateTimeOffset? createdAfter = Cursor.FromCursor<DateTimeOffset?>(pageOptions.Before);
        DateTimeOffset? createdBefore = Cursor.FromCursor<DateTimeOffset?>(pageOptions.After);
        int? first = pageOptions.First;
        int? last = pageOptions.Last;

        ICommandResult<CursorPaginatedResult<Tenant>> result = await _mediator
            .Send(new GetTenantAllPageCommand(first, last, createdAfter, createdBefore), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                CursorPaginatedResult<Tenant> resultBm = result.Data;

                (string startCursor, string endCursor) = Cursor.GetFirstAndLastCursor(resultBm.Items, x => x.CreatedDateTime);

                List<TenantVm> items = _tenantVmMapper.MapList(resultBm.Items);

                PaginatedResultVm<TenantVm> paginatedResultVm = _paginatedResultFactory
                    .GetCursorPaginatedResult(pageOptions, resultBm.HasNextPage, resultBm.HasPreviousPage,
                    resultBm.TotalCount, startCursor, endCursor, RouteNames.GetTenantPage, items);

                _actionCtx.ActionContext.HttpContext
                    .Response.Headers.Add(YaHeaderKeys.Link, paginatedResultVm.PageInfo.ToLinkHttpHeaderValue());

                return new OkObjectResult(paginatedResultVm);
        }
    }
}
