using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Application.ActionFilters
{
    public sealed class LoggingFilter : ActionFilterAttribute
    {
        public LoggingFilter(ILogger<LoggingFilter> logger, IActionContextAccessor actionContextAccessor)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        }

        private readonly ILogger<LoggingFilter> _log;
        private readonly IActionContextAccessor _actionContextAccessor;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromActionContext();

            using (_log.BeginScopeWith((Logs.CorrelationId, correlationId)))
            {
                try
                {
                    await next();
                }
                catch (Exception e) when (_log.LogException(e))
                {
                    throw;
                }
            }
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next.Invoke();
        }
    }
}
