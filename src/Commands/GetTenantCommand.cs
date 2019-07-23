using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.TenantWorker.DAL;
using YA.TenantWorker.Models;
using YA.TenantWorker.ViewModels;
using Delobytes.Mapper;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Commands
{
    public class GetTenantCommand : IGetTenantCommand
    {
        public GetTenantCommand(ILogger<GetTenantCommand> logger, IActionContextAccessor actionContextAccessor, ITenantManagerDbContext managerDbContext, IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _managerDbContext = managerDbContext ?? throw new ArgumentNullException(nameof(managerDbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<GetTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantManagerDbContext _managerDbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromContext();

            using (_log.BeginScopeWith((Logs.TenantId, tenantId), (Logs.CorrelationId, correlationId)))
            {
                Tenant tenant = await _managerDbContext.GetTenantAsync(tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new NotFoundResult();
                }

                HttpContext httpContext = _actionContextAccessor.ActionContext.HttpContext;

                if (httpContext.Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= tenant.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                TenantVm tenantViewModel = _tenantVmMapper.Map(tenant);
                httpContext.Response.Headers.Add(HeaderNames.LastModified, tenant.LastModifiedDateTime.ToString("R"));

                return new OkObjectResult(tenantViewModel);
            }
        }
    }
}
