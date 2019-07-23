using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YA.TenantWorker.Commands;
using YA.TenantWorker.DAL;
using YA.TenantWorker.Models;

namespace YA.TenantWorker.ActionFilters
{
    public class GetTenantRouteAttribute : IActionFilter
    {
        public GetTenantRouteAttribute(TenantManagerDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly TenantManagerDbContext _dbContext;

        public void OnActionExecuting(ActionExecutingContext context)
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

            Tenant tenant = _dbContext.Tenants.SingleOrDefault(x => x.TenantID.Equals(tenantId));

            if (tenant == null)
            {
                context.Result = new NotFoundResult();
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

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
