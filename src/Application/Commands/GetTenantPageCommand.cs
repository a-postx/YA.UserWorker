using Delobytes.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
            IActionContextAccessor actionContextAccessor,
            IPagingLinkHelper pagingLinkHelper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _pagingLinkHelper = pagingLinkHelper ?? throw new ArgumentNullException(nameof(pagingLinkHelper));
        }

        private readonly ILogger<GetTenantPageCommand> _log;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IPagingLinkHelper _pagingLinkHelper;

        public async Task<IActionResult> ExecuteAsync(PageOptions pageOptions, CancellationToken cancellationToken)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromActionContext();

            if (correlationId == Guid.Empty)
            {
                return new BadRequestResult();
            }

            using (_log.BeginScopeWith((Logs.CorrelationId, correlationId)))
            {
                try
                {
                    ICollection<Tenant> tenants = await _dbContext
                        .GetEntitiesOrderedAndPagedAsync<Tenant>(e => e.TenantID, pageOptions.Page.Value, pageOptions.Count.Value, cancellationToken);

                    if (tenants == null)
                    {
                        return new NotFoundResult();
                    }

                    (int totalCount, int totalPages) = await _dbContext.GetTotalPagesAsync<Tenant>(pageOptions.Count.Value, cancellationToken);
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
                    _actionContextAccessor.ActionContext.HttpContext
                        .Response.Headers.Add("Link", _pagingLinkHelper.GetLinkValue(page, RouteNames.GetTenantPage));

                    return new OkObjectResult(page);
                }
                catch (Exception e) when (_log.LogException(e))
                {
                    throw;
                } 
            }
        }
    }
}
