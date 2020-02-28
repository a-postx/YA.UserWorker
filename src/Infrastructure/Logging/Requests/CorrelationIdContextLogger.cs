using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.Common;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// CorrelationID context logging middleware. 
    /// </summary>
    public class CorrelationIdContextLogger
    {
        public CorrelationIdContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<CorrelationIdContextLogger> logger, IRuntimeContextAccessor runtimeContextAccessor)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Guid correlationId = runtimeContextAccessor.GetCorrelationId();

            if (correlationId != Guid.Empty)
            {
                using (logger.BeginScopeWith((Logs.CorrelationId, correlationId.ToString())))
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
