using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    public class YaAuthenticationHandler : IAuthenticationHandler
    {
        public YaAuthenticationHandler(ILogger<YaAuthenticationHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private readonly ILogger<YaAuthenticationHandler> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private AuthenticationScheme _scheme;
        private RequestHeaders _headers;

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            RequestHeaders headers = context.Request.GetTypedHeaders();

            if (scheme != null && headers != null)
            {
                _scheme = scheme;
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
                string[] authHeaderValues = authValue.ToString().Split(" ");

                string token = (authHeaderValues.Length > 1) ? authHeaderValues[1] : authHeaderValues[0];

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken tokenS = handler.ReadJwtToken(token);

                if (tokenS != null)
                {
                    string tenantId = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.tid)?.Value;
                    string userId = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.nameidentifier)?.Value;
                    string username = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.authsub)?.Value;
                    string name = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.name)?.Value;
                    string useremail = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.authemail)?.Value;
                    string userrole = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.role)?.Value;

                    if (tenantId == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.tid} claim cannot be found.");
                    }
                    if (userId == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.nameidentifier} claim cannot be found.");
                    }
                    if (username == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.authsub} claim cannot be found.");
                    }
                    if (name == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.name} claim cannot be found.");
                    }
                    if (useremail == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.authemail} claim cannot be found.");
                    }
                    if (userrole == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.role} claim cannot be found.");
                    }

                    ClaimsIdentity userIdentity = new ClaimsIdentity("Bearer", CustomClaimNames.name, CustomClaimNames.role);

                    userIdentity.AddClaim(new Claim(CustomClaimNames.tid, tenantId));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.nameidentifier, userId));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.username, username));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.name, name));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.useremail, useremail));
                    GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { userrole });
                    ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                    _log.LogInformation("User {Username} is authenticated.", username);

                    return Task.FromResult(
                        AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), _scheme.Name))
                    );
                }
                else
                {
                    return Task.FromResult(AuthenticateResult.Fail(new Exception("Cannot get security token.")));
                }
            }
            else
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            context.Response.Redirect("/authentication");
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    }
}
