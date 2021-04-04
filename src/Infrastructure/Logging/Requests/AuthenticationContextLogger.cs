using System;
using System.Threading.Tasks;
using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YA.Common.Constants;
using YA.UserWorker.Extensions;

namespace YA.UserWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Прослойка логирования контекста аутентификации. 
    /// </summary>
    public class AuthenticationContextLogger
    {
        public AuthenticationContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ILogger<AuthenticationContextLogger> logger)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            
            if (context.User.Identity.IsAuthenticated)
            {
                using (logger.BeginScopeWith((YaLogKeys.TenantId, context.User.GetClaimValue<Guid>(YaClaimNames.tid)),
                    (YaLogKeys.UserId, context.User.GetClaimValue<string>(YaClaimNames.uid))))
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
