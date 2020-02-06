using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using YA.Common;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Network context logging middleware. 
    /// </summary>
    public class NetworkContextLogger
    {
        public NetworkContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<CorrelationIdContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            if (context.Request.Headers.TryGetValue(General.ForwardedForHeader, out StringValues forwardedValue))
            {
                string clientIp = forwardedValue.ToString().Split(':')[0];

                using (logger.BeginScopeWith((Logs.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : "unknown")))
                {
                    await _next(context);
                }
            }
            else
            {
                string clientIp = httpContext.Connection.RemoteIpAddress.ToString();

                using (logger.BeginScopeWith((Logs.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : "unknown")))
                {
                    await _next(context);
                }
            }
        }
    }
}
