﻿using CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// CorrelationId context logging middleware. 
    /// </summary>
    public class CorrelationIdContextLogger
    {
        public CorrelationIdContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<CorrelationIdContextLogger> logger, ICorrelationContextAccessor correlationContextAccessor)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            string correlationId = correlationContextAccessor.CorrelationContext.CorrelationId;

            if (!string.IsNullOrEmpty(correlationId))
            {
                using (logger.BeginScopeWith((Logs.CorrelationId, correlationId)))
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
