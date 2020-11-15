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

        public async Task InvokeAsync(HttpContext httpContext, ILogger<NetworkContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            string clientIp;

            if (context.Request.Headers.TryGetValue("X-Original-For", out StringValues proxyForwardedValue))
            {
                clientIp = GetIpFromHeaderString(proxyForwardedValue);
            }
            else if (context.Request.Headers.TryGetValue("X-Client-IP", out StringValues azureValue))
            {
                clientIp = GetIpFromHeaderString(azureValue);
            }
            else
            {
                string remoteAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                clientIp = !string.IsNullOrEmpty(remoteAddress) ? remoteAddress : "unknown";
            }

            using (logger.BeginScopeWith((YaLogKeys.ClientIP, clientIp)))
            {
                await _next(context);
            }
        }

        private static string GetIpFromHeaderString(StringValues ipAddresses)
        {
            string ipAddress = ipAddresses.Last().Split(',').First().Trim();
            var portDelimiterPos = ipAddress.LastIndexOf(":", StringComparison.CurrentCultureIgnoreCase);

            if (portDelimiterPos != -1)
            {
                ipAddress = ipAddress.Substring(0, portDelimiterPos);
            }

            return ipAddress;
        }
    }
}
