﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using YA.Common.Constants;
using YA.TenantWorker.Constants;
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

        readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<NetworkContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            if (context.Request.Headers.TryGetValue(General.ForwardedForHeader, out StringValues forwardedValue))
            {
                string clientIp = forwardedValue.ToString().Split(':')[0];

                using (logger.BeginScopeWith((YaLogKeys.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : "unknown")))
                {
                    await _next(context);
                }
            }
            else
            {
                string clientIp = httpContext.Connection.RemoteIpAddress.ToString();

                using (logger.BeginScopeWith((YaLogKeys.ClientIP, !string.IsNullOrEmpty(clientIp) ? clientIp : "unknown")))
                {
                    await _next(context);
                }
            }
        }
    }
}
