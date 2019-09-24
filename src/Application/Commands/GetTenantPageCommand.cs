using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Delobytes.Mapper;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Commands
{
    public class GetTenantPageCommand : IGetTenantPageCommand
    {
        public GetTenantPageCommand(ILogger<GetTenantPageCommand> logger, ITenantWorkerDbContext workerDbContext, IMapper<Tenant, TenantVm> tenantVmMapper, IHttpContextAccessor httpContextAccessor, IPagingLinkHelper pagingLinkHelper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantWorkerDbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _pagingLinkHelper = pagingLinkHelper ?? throw new ArgumentNullException(nameof(pagingLinkHelper));
        }

        private readonly ILogger<GetTenantPageCommand> _log;
        private readonly ITenantWorkerDbContext _tenantWorkerDbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagingLinkHelper _pagingLinkHelper;

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            ICollection<Tenant> tenants = await _tenantWorkerDbContext
                .GetEntitiesOrderedAndPagedAsync<Tenant>(e => e.TenantID, pageOptions.Page.Value, pageOptions.Count.Value, cancellationToken);

            if (tenants == null)
            {
                return new NotFoundResult();
            }

            var (totalCount, totalPages) = await _tenantWorkerDbContext.GetTotalPagesAsync<Tenant>(pageOptions.Count.Value, cancellationToken);
            List<TenantVm> tenantVms = _tenantVmMapper.MapList(tenants);

            PageResult<TenantVm> page = new PageResult<TenantVm>()
            {
                Count = pageOptions.Count.Value,
                Items = tenantVms,
                Page = pageOptions.Page.Value,
                TotalCount = totalCount,
                TotalPages = totalPages,
            };

            // Add the Link HTTP Header to add URL's to next, previous, first and last pages.
            // See https://tools.ietf.org/html/rfc5988#page-6
            // There is a standard list of link relation types e.g. next, previous, first and last.
            // See https://www.iana.org/assignments/link-relations/link-relations.xhtml
            _httpContextAccessor.HttpContext.Response.Headers.Add("Link", _pagingLinkHelper.GetLinkValue(page, RouteNames.GetTenantPage));

            return new OkObjectResult(page);
        }
    }
}
