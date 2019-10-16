using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YA.TenantWorker.Application.Commands;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.ActionFilters
{
    public sealed class TenantRouteFilter : ActionFilterAttribute
    {
        public TenantRouteFilter(ITenantWorkerDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ITenantWorkerDbContext _dbContext;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Guid tenantId;

            if (context.ActionArguments.ContainsKey("tenantId"))
            {
                bool success = Guid.TryParse(context.ActionArguments["tenantId"].ToString(), out Guid id);

                if (success)
                {
                    tenantId = id;
                }
                else
                {
                    context.Result = new BadRequestObjectResult("Bad tenantId parameter");
                    return;
                }
            }
            else
            {
                context.Result = new BadRequestObjectResult("Bad tenantId parameter");
                return;
            }

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Tenant tenant = await _dbContext.GetEntityAsync<Tenant>(e => e.TenantID.Equals(tenantId), cts.Token);

                if (tenant == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }
                else
                {
                    RouteEntities entities = new RouteEntities
                    {
                        Tenant = tenant
                    };

                    context.HttpContext.Items.Add(nameof(RouteEntities), entities);
                }
            }

            await next.Invoke();
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next.Invoke();
        }
    }
}
