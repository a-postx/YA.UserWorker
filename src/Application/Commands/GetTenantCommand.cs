using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.ViewModels;
using Delobytes.Mapper;

namespace YA.TenantWorker.Application.Commands
{
    public class GetTenantCommand : IGetTenantCommand
    {
        public GetTenantCommand(ILogger<GetTenantCommand> logger,
            IRuntimeContextAccessor runtimeContextAccessor,
            IActionContextAccessor actionContextAccessor,
            ITenantWorkerDbContext dbContext,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeContext = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<GetTenantCommand> _log;
        private readonly IRuntimeContextAccessor _runtimeContext;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);
            //await Task.Delay(6000);
            if (tenant == null)
            {
                return new NotFoundResult();
            }
            
            if (_actionContextAccessor.ActionContext.HttpContext
                .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
            {
                if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= tenant.LastModifiedDateTime))
                {
                    return new StatusCodeResult(StatusCodes.Status304NotModified);
                }
            }

            TenantVm tenantViewModel = _tenantVmMapper.Map(tenant);
            _actionContextAccessor.ActionContext.HttpContext
                .Response.Headers.Add(HeaderNames.LastModified, tenant.LastModifiedDateTime.ToString("R"));
                    
            return new OkObjectResult(tenantViewModel);
        }
    }
}
