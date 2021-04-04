using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using YA.Common.Constants;
using YA.UserWorker.Extensions;

namespace YA.UserWorker.Infrastructure.Logging.Requests
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
                string directValue = httpContext.Connection.RemoteIpAddress?.ToString();
                clientIp = !string.IsNullOrEmpty(directValue) ? directValue : "0.0.0.0";
            }

            using (logger.BeginScopeWith((YaLogKeys.ClientIP, clientIp)))
            {
                await _next(context);
            }
        }

        private static string GetIpFromHeaderString(StringValues ipAddresses)
        {
            string[] addresses = ipAddresses.LastOrDefault().Split(',');

            if (addresses.Length != 0)
            {
                return addresses[0].Contains(":", StringComparison.Ordinal)
                    ? addresses[0].Substring(0, addresses[0].LastIndexOf(":", StringComparison.Ordinal))
                    : addresses[0];
            }

            return string.Empty;
        }
    }
}
