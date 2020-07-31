using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    public class YaAuthenticationHandler : IAuthenticationHandler
    {
        public YaAuthenticationHandler(ILogger<YaAuthenticationHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IConfigurationManager<OpenIdConnectConfiguration> configManager)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));            
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        }

        private readonly ILogger<YaAuthenticationHandler> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;
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

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            if (_headers.Headers.TryGetValue(HeaderNames.Authorization, out StringValues authValue))
            {
                string[] authHeaderValues = authValue.ToString().Split(" ");

                string token = (authHeaderValues.Length > 1) ? authHeaderValues[1] : authHeaderValues[0];

                //todo: добавить опцию выбора "валидировать ли токен" на случай проблем
                ////JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                ////JwtSecurityToken validatedToken = handler.ReadJwtToken(token);

                JwtSecurityToken validatedToken;

                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    validatedToken = await ValidateTokenAsync(token, cts.Token);
                }

                if (validatedToken != null)
                {
                    string userId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.uid)?.Value;
                    string username = validatedToken.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.sub)?.Value;
                    string email = validatedToken.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.email)?.Value;
                    string name = validatedToken.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.name)?.Value;

                    if (userId == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.uid} claim cannot be found.");
                    }
                    if (username == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.username} claim cannot be found.");
                    }
                    if (email == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.email} claim cannot be found.");
                    }
                    if (name == null)
                    {
                        throw new Exception($"Authentication failed: {CustomClaimNames.name} claim cannot be found.");
                    }

                    ClaimsIdentity userIdentity = new ClaimsIdentity("Bearer", CustomClaimNames.name, CustomClaimNames.role);

                    Guid tenantId = TenantIdGenerator.Create(userId);
                    ////Guid tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                    userIdentity.AddClaim(new Claim(CustomClaimNames.uid, userId));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.tid, tenantId.ToString()));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.username, username));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.email, email));
                    userIdentity.AddClaim(new Claim(CustomClaimNames.name, name));
                    GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { "user" });
                    ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                    AuthenticationProperties props = new AuthenticationProperties();
                    props.IssuedUtc = validatedToken.IssuedAt;
                    props.ExpiresUtc = validatedToken.ValidTo;
                    props.RedirectUri = General.LoginRedirectPath;

                    _log.LogInformation("User {Username} is authenticated.", username);

                    return  AuthenticateResult.Success(new AuthenticationTicket(principal, props, _scheme.Name));
                }
                else
                {
                    return AuthenticateResult.Fail(new Exception("Cannot validate security token."));
                }
            }
            else
            {
                return AuthenticateResult.NoResult();
            }
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            
            AppSecrets secrets = _config.Get<AppSecrets>();

            if (context.Request.Host.Host == secrets.ApiGatewayHost && context.Request.Host.Port == secrets.ApiGatewayPort)
            {
                _log.LogInformation("Challenge: redirected.");
                context.Response.Redirect(General.LoginRedirectPath);
                return Task.CompletedTask;
            }
            else
            {
                _log.LogInformation("Challenge: unauthorized.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpContextAccessor.HttpContext;
            _log.LogInformation("Forbid: forbidden.");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        private async Task<JwtSecurityToken> ValidateTokenAsync(string token, CancellationToken cancellationToken)
        {
            string issuer = _config.Get<AppSecrets>().OidcProviderIssuer;
            if (string.IsNullOrEmpty(issuer))
            {
                throw new ApplicationException("Issuer is empty.");
            }

            //значение кешировано большую часть времени (3мс), поэтому проблемы производительности быть не должно
            OpenIdConnectConfiguration discoveryDocument = await _configManager.GetConfigurationAsync(cancellationToken);
            System.Collections.Generic.ICollection<SecurityKey> signingKeys = discoveryDocument.SigningKeys;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ////ValidAudience = "",
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            try
            {
                ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out SecurityToken rawValidatedToken);

                JwtSecurityToken validatedToken = (JwtSecurityToken)rawValidatedToken;

                string expectedAlg = SecurityAlgorithms.RsaSha256; //Okta uses RS256

                if (validatedToken.Header?.Alg == null || validatedToken.Header?.Alg != expectedAlg)
                {
                    throw new SecurityTokenValidationException($"The alg must be {expectedAlg}.");
                }

                return validatedToken;
            }
            catch (SecurityTokenExpiredException)
            {
                _log.LogInformation("Security token expired.");
                return null;
            }
            catch (SecurityTokenValidationException ex)
            {
                _log.LogWarning(ex, "Security token validation failed for JWT {Token}.", token);
                return null;
            }
        }
    }
}
