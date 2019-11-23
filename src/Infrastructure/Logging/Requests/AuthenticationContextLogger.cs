using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Authentication context logging middleware. 
    /// </summary>
    public class AuthenticationContextLogger
    {
        public AuthenticationContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<AuthenticationContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            
            if (context.User.Identity.IsAuthenticated)
            {
                using (logger.BeginScopeWith((Logs.TenantId, context.User.GetClaimValue<Guid>(CustomClaimNames.tenant_id))))
                using (logger.BeginScopeWith((Logs.UserId, context.User.GetClaimValue<Guid>(CustomClaimNames.nameidentifier))))
                using (logger.BeginScopeWith((Logs.Username, context.User.GetClaimValue<string>(CustomClaimNames.username))))
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
