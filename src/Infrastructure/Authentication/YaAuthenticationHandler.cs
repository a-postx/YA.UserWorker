using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    public class YaAuthenticationHandler : IAuthenticationHandler
    {
        public YaAuthenticationHandler(ILogger<YaAuthenticationHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private readonly ILogger<YaAuthenticationHandler> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
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
                    string userId = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.uid)?.Value;
                    string username = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.sub)?.Value;
                    string name = tokenS.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.name)?.Value;

                    if (userId == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.uid} claim cannot be found.");
                    }
                    if (username == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.username} claim cannot be found.");
                    }
                    if (name == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.name} claim cannot be found.");
                    }

                    ClaimsIdentity userIdentity = new ClaimsIdentity("Bearer", CustomClaimNames.name, CustomClaimNames.role);

                    Guid tenantId;
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(userId));
                        tenantId = new Guid(hash);
                    }
                    ////Guid tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                    userIdentity.AddClaim(new Claim(CustomClaimNames.uid, userId));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.tid, tenantId.ToString()));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.username, username));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.name, name));
                    GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { "user" });
                    ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                    AuthenticationProperties props = new AuthenticationProperties();
                    props.IssuedUtc = tokenS.IssuedAt;
                    props.ExpiresUtc = tokenS.ValidTo;
                    props.RedirectUri = General.LoginRedirectPath;

                    _log.LogInformation("User {Username} is authenticated.", username);

                    return Task.FromResult(
                        AuthenticateResult.Success(new AuthenticationTicket(principal, props, _scheme.Name))
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

            AppSecrets secrets = _configuration.Get<AppSecrets>();

            if (context.Request.Host.Host == secrets.ApiGatewayHost && context.Request.Host.Port == secrets.ApiGatewayPort)
            {
                context.Response.Redirect(General.LoginRedirectPath);
                return Task.CompletedTask;
            }
            else
            {
                return ForbidAsync(properties);
            }
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    }
}
