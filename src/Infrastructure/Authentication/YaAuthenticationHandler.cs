using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using YA.Common;
using YA.Common.Constants;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    /// <summary>
    /// Обработчик аутентификации АПИ-запроса
    /// </summary>
    public class YaAuthenticationHandler : IAuthenticationHandler
    {
        public YaAuthenticationHandler(ILogger<YaAuthenticationHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppSecrets> secrets,
            IConfigurationManager<OpenIdConnectConfiguration> configManager,
            IOptions<OauthOptions> oauthOptions)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpCtx = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            AppSecrets appSecrets = secrets.Value;
            _apiGwHost = appSecrets.ApiGatewayHost ?? throw new ArgumentNullException(nameof(appSecrets.ApiGatewayHost));
            _apiGwPort = appSecrets.ApiGatewayPort == 0 ? throw new ArgumentNullException(nameof(appSecrets.ApiGatewayPort)) : appSecrets.ApiGatewayPort;
            
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _oauthOptions = oauthOptions.Value;
        }

        private readonly ILogger<YaAuthenticationHandler> _log;
        private readonly IHttpContextAccessor _httpCtx;
        private readonly string _apiGwHost;
        private readonly int _apiGwPort;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private readonly OauthOptions _oauthOptions;
        private AuthenticationScheme _scheme;
        private RequestHeaders _headers;

        private const string _authType = "Bearer";
        private const string _loginRedirectPath = "/authentication/login";

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
            if (JwtTokenFound(out string token))
            {
                JwtSecurityToken validatedToken;

                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(Timeouts.SecurityTokenValidationSec)))
                {
                    validatedToken = await ValidateTokenAsync(token, cts.Token);
                }

                if (validatedToken != null)
                {
                    string clientId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.azp)?.Value;
                    string userId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.sub)?.Value;
                    string userName = validatedToken.Claims.FirstOrDefault(claim => claim.Type == "http://yaapp.name")?.Value;
                    string email = validatedToken.Claims.FirstOrDefault(claim => claim.Type == "http://yaapp.email")?.Value;
                    string emailVerified = validatedToken.Claims.FirstOrDefault(claim => claim.Type == "http://yaapp.email_verified")?.Value;

                    if (string.IsNullOrEmpty(clientId))
                    {
                        return AuthenticateResult.Fail($"{YaClaimNames.azp} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(userId))
                    {
                        return AuthenticateResult.Fail($"{YaClaimNames.uid} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(userName))
                    {
                        return AuthenticateResult.Fail($"{YaClaimNames.name} claim cannot be found.");
                    }

                    ClaimsIdentity userIdentity = new ClaimsIdentity(_authType, YaClaimNames.name, YaClaimNames.role);

                    Guid tenantId = TenantIdGenerator.Create(userId);
                    ////Guid tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                    userIdentity.AddClaim(new Claim(YaClaimNames.cid, clientId));
                    userIdentity.AddClaim(new Claim(YaClaimNames.uid, userId));
                    userIdentity.AddClaim(new Claim(YaClaimNames.tid, tenantId.ToString()));
                    userIdentity.AddClaim(new Claim(YaClaimNames.name, userName));

                    if (email != null)
                    {
                        userIdentity.AddClaim(new Claim(YaClaimNames.email, email));
                    }

                    if (emailVerified != null)
                    {
                        userIdentity.AddClaim(new Claim(YaClaimNames.emailVerified, emailVerified));
                    }

                    GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { "user" });
                    ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                    AuthenticationProperties props = new AuthenticationProperties();
                    props.IssuedUtc = validatedToken.IssuedAt;
                    props.ExpiresUtc = validatedToken.ValidTo;
                    props.RedirectUri = _loginRedirectPath;

                    _log.LogInformation("User {UserId} is authenticated.", userId);

                    return AuthenticateResult.Success(new AuthenticationTicket(principal, props, _scheme.Name));
                }
                else
                {
                    return AuthenticateResult.Fail("Cannot validate security token.");
                }
            }
            else
            {
                return AuthenticateResult.NoResult();
            }
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            HttpContext context = _httpCtx.HttpContext;

            if (context.Request.Host.Host == _apiGwHost && context.Request.Host.Port == _apiGwPort)
            {
                _log.LogInformation("Challenge: redirected.");
                context.Response.Redirect(_loginRedirectPath);
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
            HttpContext context = _httpCtx.HttpContext;
            _log.LogInformation("Forbid: forbidden.");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        private bool JwtTokenFound(out string token)
        {
            bool tokenFound = false;
            token = null;

            if (_headers.Headers.TryGetValue(HeaderNames.Authorization, out StringValues authHeaders) && authHeaders.Any())
            {
                string tokenHeaderValue = authHeaders.ElementAt(0);
                token = tokenHeaderValue.StartsWith(_authType + " ", StringComparison.OrdinalIgnoreCase)
                    ? tokenHeaderValue.Substring(7) : tokenHeaderValue;
                tokenFound = true;
            }

            return tokenFound;
        }

        private async Task<JwtSecurityToken> ValidateTokenAsync(string token, CancellationToken cancellationToken)
        {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

            try
            {
                jwtHandler.ReadToken(token);
            }
            catch (ArgumentNullException)
            {
                _log.LogInformation("Security token is null.");
                return null;
            }
            catch (ArgumentException)
            {
                _log.LogInformation("Security token is not well formed.");
                return null;
            }

            TokenValidationParameters validationParameters = await GetValidationParametersAsync(cancellationToken);

            try
            {
                ClaimsPrincipal principal = jwtHandler.ValidateToken(token, validationParameters, out SecurityToken rawValidatedToken);

                JwtSecurityToken validatedToken = (JwtSecurityToken)rawValidatedToken;
                string expectedAlg = SecurityAlgorithms.RsaSha256;

                if (validatedToken.Header?.Alg == null || validatedToken.Header?.Alg != expectedAlg)
                {
                    throw new SecurityTokenValidationException($"The security token alg must be {expectedAlg}.");
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
                _log.LogInformation($"Security token validation failed: {ex.Message}");
                return null;
            }
            catch (SecurityTokenException)
            {
                _log.LogWarning("Security token exception.");
                return null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Security token validation failed with exception.");
                return null;
            }
        }

        private async Task<TokenValidationParameters> GetValidationParametersAsync(CancellationToken cancellationToken)
        {
            //значение кешировано большую часть времени (3мс), поэтому проблемы производительности быть не должно
            OpenIdConnectConfiguration discoveryDocument = await _configManager.GetConfigurationAsync(cancellationToken);
            ICollection<SecurityKey> signingKeys = discoveryDocument.SigningKeys;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = _oauthOptions.Authority + "/",
                ValidateAudience = true,
                ValidAudience = _oauthOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            return validationParameters;
        }
    }
}
