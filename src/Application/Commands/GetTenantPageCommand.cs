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
    public class GetTenantPageCommand : IGetTenantPageCommand
    {
        public GetTenantPageCommand(ILogger<GetTenantPageCommand> logger,
            ITenantWorkerDbContext workerDbContext,
            IMapper<Tenant, TenantVm> tenantVmMapper,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        }
        
        private readonly ILogger<GetTenantPageCommand> _log;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            Guid correlationId = _httpContextAccessor.GetCorrelationIdFromIHttpContext();

            pageOptions.First = !pageOptions.First.HasValue && !pageOptions.Last.HasValue ? General.DefaultPageSizeForPagination : pageOptions.First;
            DateTimeOffset? createdAfter = Cursor.FromCursor<DateTimeOffset?>(pageOptions.After);
            DateTimeOffset? createdBefore = Cursor.FromCursor<DateTimeOffset?>(pageOptions.Before);

            Task<List<Tenant>> getTenantsTask = _dbContext
                .GetItemsTask<Tenant>(pageOptions.First, pageOptions.Last, createdAfter, createdBefore, cancellationToken);
            Task<bool> getHasNextPageTask = _dbContext
                .GetHasNextPage<Tenant>(pageOptions.First, createdAfter, createdBefore, cancellationToken);
            Task<bool> getHasPreviousPageTask = _dbContext
                .GetHasPreviousPage<Tenant>(pageOptions.Last, createdAfter, createdBefore, cancellationToken);
            Task<int> totalCountTask = _dbContext.GetTotalItemsCountAsync<Tenant>(cancellationToken);

            await Task.WhenAll(getTenantsTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);

            List<Tenant> tenants = getTenantsTask.Result;
            bool hasNextPage = getHasNextPageTask.Result;
            bool hasPreviousPage = getHasPreviousPageTask.Result;
            int totalCount = totalCountTask.Result;

            if (tenants == null)
            {
                return new NotFoundResult();
            }

            (string startCursor, string endCursor) = Cursor.GetFirstAndLastCursor(tenants, x => x.CreatedDateTime);

            List<TenantVm> tenantVms = _tenantVmMapper.MapList(tenants);
            PaginatedResult<TenantVm> paginatedResult= new PaginatedResult<TenantVm>(_linkGenerator, pageOptions, hasNextPage,
                hasPreviousPage, totalCount, startCursor, endCursor, _httpContextAccessor.HttpContext, RouteNames.GetTenantPage, tenantVms);

            _httpContextAccessor.HttpContext.Response.Headers.Add(CustomHeaderNames.Link, paginatedResult.PageInfo.ToLinkHttpHeaderValue());

            return new OkObjectResult(paginatedResult);
        }
    }
}
