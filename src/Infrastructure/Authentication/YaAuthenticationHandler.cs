using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    public class YaAuthenticationHandler : IAuthenticationHandler
    {
        public YaAuthenticationHandler(ILogger<YaAuthenticationHandler> logger)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly ILogger<YaAuthenticationHandler> _log;
        private AuthenticationScheme _scheme;
        //убрать поле контекста, если появится многопоточность
        private HttpContext _context;
        private RequestHeaders _headers;

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            RequestHeaders headers = context.Request.GetTypedHeaders();

            if (scheme != null && context != null && headers != null)
            {
                _scheme = scheme;
                _context = context;
                _headers = headers;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            if (_headers.Headers.TryGetValue(HeaderNames.Authorization, out StringValues authValue))
            {
                bool gotTenantId = _headers.Headers.TryGetValue(CustomHeaderNames.claims_tenant_id, out StringValues tenantIdValue);
                bool gotUserId = _headers.Headers.TryGetValue(CustomHeaderNames.claims_nameidentifier, out StringValues userIdValue);
                bool gotUsername = _headers.Headers.TryGetValue(CustomHeaderNames.claims_authsub, out StringValues usernameValue);
                bool gotName = _headers.Headers.TryGetValue(CustomHeaderNames.claims_name, out StringValues nameValue);
                bool gotUserEmail = _headers.Headers.TryGetValue(CustomHeaderNames.claims_authemail, out StringValues userEmailValue);

                if (!gotTenantId)
                {
                    throw new Exception($"Authentication failed: {CustomHeaderNames.claims_tenant_id} claim cannot be found.");
                }
                if (!gotUserId)
                {
                    throw new Exception($"Authentication failed: {CustomHeaderNames.claims_nameidentifier} claim cannot be found.");
                }
                if (!gotUsername)
                {
                    throw new Exception($"Authentication failed: {CustomHeaderNames.claims_authsub} claim cannot be found.");
                }
                if (!gotName)
                {
                    throw new Exception($"Authentication failed: {CustomHeaderNames.claims_name} claim cannot be found.");
                }
                if (!gotUserEmail)
                {
                    throw new Exception($"Authentication failed: {CustomHeaderNames.claims_authemail} claim cannot be found.");
                }

                ClaimsIdentity userIdentity = new ClaimsIdentity("Header", "name", "role");
                
                userIdentity.AddClaim(new Claim(CustomClaimNames.tenant_id, tenantIdValue));
                userIdentity.AddClaim(new Claim(CustomClaimNames.nameidentifier, userIdValue));
                userIdentity.AddClaim(new Claim(CustomClaimNames.username, usernameValue));
                userIdentity.AddClaim(new Claim(CustomClaimNames.name, nameValue));
                userIdentity.AddClaim(new Claim(CustomClaimNames.useremail, userEmailValue));
                GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { "Administrator" });
                ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                _log.LogInformation("User {Username} is authenticated.", usernameValue);

                return Task.FromResult(
                    AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), _scheme.Name))
                );
            }
            else
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            _context.Response.Redirect("/authentication");
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            _context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }
}
