using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using YA.Common.Constants;
using YA.TenantWorker.Extensions;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Прослойка логирования сетевого контекста. 
    /// </summary>
    public class NetworkContextLogger
    {
        public NetworkContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;
        private readonly string _unknownIp = "unknown";

        public async Task InvokeAsync(HttpContext httpContext, ILogger<NetworkContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            if (context.Request.Headers.TryGetValue("X-Original-For", out StringValues forwardedValue))
            {
                string clientIp = null;

                string clientIpsList = forwardedValue.ToString();
                string clientIpElement = clientIpsList.Split(',').Select(s => s.Trim()).FirstOrDefault();

                if (!string.IsNullOrEmpty(clientIpElement))
                {
                    clientIp = clientIpElement.Split(':').Select(s => s.Trim()).FirstOrDefault();
                }

                using (logger.BeginScopeWith((YaLogKeys.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : _unknownIp)))
                {
                    await _next(context);
                }
            }
            else
            {
                string clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

                using (logger.BeginScopeWith((YaLogKeys.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : _unknownIp)))
                {
                    await _next(context);
                }
            }
        }
    }
}
