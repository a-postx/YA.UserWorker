using Delobytes.AspNetCore;
using Delobytes.Mapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class GetTenantAllPageCommand : IGetTenantAllPageCommand
    {
        public GetTenantAllPageCommand(ILogger<GetTenantAllPageCommand> logger,
            ITenantWorkerDbContext dbContext,
            IMapper<Tenant, TenantVm> tenantVmMapper,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        }
        
        private readonly ILogger<GetTenantAllPageCommand> _log;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            pageOptions.First = !pageOptions.First.HasValue && !pageOptions.Last.HasValue ? General.DefaultPageSizeForPagination : pageOptions.First;
            DateTimeOffset? createdAfter = Cursor.FromCursor<DateTimeOffset?>(pageOptions.After);
            DateTimeOffset? createdBefore = Cursor.FromCursor<DateTimeOffset?>(pageOptions.Before);

            Task<List<Tenant>> getItemsTask = _dbContext
                .GetEntitiesPagedTaskAsync<Tenant>(pageOptions.First, pageOptions.Last, createdAfter, createdBefore, cancellationToken);
            Task<bool> getHasNextPageTask = _dbContext
                .GetHasNextPageAsync<Tenant>(pageOptions.First, createdAfter, createdBefore, cancellationToken);
            Task<bool> getHasPreviousPageTask = _dbContext
                .GetHasPreviousPageAsync<Tenant>(pageOptions.Last, createdAfter, createdBefore, cancellationToken);
            Task<int> totalCountTask = _dbContext.GetEntitiesCountAsync<Tenant>(cancellationToken);

            await Task.WhenAll(getItemsTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);

            List<Tenant> items = await getItemsTask;
            bool hasNextPage = await getHasNextPageTask;
            bool hasPreviousPage = await getHasPreviousPageTask;
            int totalCount = await totalCountTask;

            if (items == null)
            {
                return new NotFoundResult();
            }

            (string startCursor, string endCursor) = Cursor.GetFirstAndLastCursor(items, x => x.CreatedDateTime);

            List<TenantVm> itemVms = _tenantVmMapper.MapList(items);
            PaginatedResult<TenantVm> paginatedResult= new PaginatedResult<TenantVm>(_linkGenerator, pageOptions, hasNextPage,
                hasPreviousPage, totalCount, startCursor, endCursor, _httpContextAccessor.HttpContext, RouteNames.GetTenantPage, itemVms);

            _httpContextAccessor.HttpContext.Response.Headers.Add(CustomHeaderNames.Link, paginatedResult.PageInfo.ToLinkHttpHeaderValue());

            return new OkObjectResult(paginatedResult);
        }
    }
}
