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
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using YA.Common;
using YA.Common.Constants;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Authentication
{
    /// <summary>
    /// Обработчик аутентификации АПИ-запроса
    /// </summary>
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
            if (JwtTokenFound(out string token))
            {
                JwtSecurityToken validatedToken;

                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(Timeouts.SecurityTokenValidationSec)))
                {
                    validatedToken = await ValidateTokenAsync(token, cts.Token);
                }

                if (validatedToken != null)
                {
                    string clientId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.cid)?.Value;
                    string userId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.uid)?.Value;
                    string username = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.sub)?.Value;
                    string externalId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.externalId)?.Value;
                    string email = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email)?.Value;
                    string name = validatedToken.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.name)?.Value;

                    if (string.IsNullOrEmpty(clientId))
                    {
                        throw new Exception($"Authentication failed: {YaClaimNames.cid} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(userId))
                    {
                        throw new Exception($"Authentication failed: {YaClaimNames.uid} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(username))
                    {
                        throw new Exception($"Authentication failed: {YaClaimNames.username} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(email))
                    {
                        throw new Exception($"Authentication failed: {YaClaimNames.email} claim cannot be found.");
                    }
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception($"Authentication failed: {YaClaimNames.name} claim cannot be found.");
                    }

                    ClaimsIdentity userIdentity = new ClaimsIdentity("Bearer", YaClaimNames.name, YaClaimNames.role);

                    Guid tenantId = TenantIdGenerator.Create(userId);
                    ////Guid tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                    userIdentity.AddClaim(new Claim(YaClaimNames.cid, clientId));
                    userIdentity.AddClaim(new Claim(YaClaimNames.uid, userId));
                    userIdentity.AddClaim(new Claim(YaClaimNames.tid, tenantId.ToString()));
                    userIdentity.AddClaim(new Claim(YaClaimNames.username, username));
                    if (externalId != null)
                    {
                        userIdentity.AddClaim(new Claim(YaClaimNames.externalId, externalId));
                    }
                    userIdentity.AddClaim(new Claim(YaClaimNames.email, email));
                    userIdentity.AddClaim(new Claim(YaClaimNames.name, name));
                    GenericPrincipal userPricipal = new GenericPrincipal(userIdentity, new string[] { "user" });
                    ClaimsPrincipal principal = new ClaimsPrincipal(userPricipal);

                    AuthenticationProperties props = new AuthenticationProperties();
                    props.IssuedUtc = validatedToken.IssuedAt;
                    props.ExpiresUtc = validatedToken.ValidTo;
                    props.RedirectUri = General.LoginRedirectPath;

                    _log.LogInformation("User {Username} is authenticated.", username);

                    return AuthenticateResult.Success(new AuthenticationTicket(principal, props, _scheme.Name));
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

        private bool JwtTokenFound(out string token)
        {
            bool tokenFound = false;
            token = null;

            if (_headers.Headers.TryGetValue(HeaderNames.Authorization, out StringValues authHeaders) && authHeaders.Any())
            {
                string tokenHeaderValue = authHeaders.ElementAt(0);
                token = tokenHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? tokenHeaderValue.Substring(7) : tokenHeaderValue;
                tokenFound = true;
            }

            return tokenFound;
        }

        private async Task<JwtSecurityToken> ValidateTokenAsync(string token, CancellationToken cancellationToken)
        {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

            //защищаем Окту от левых запросов
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
                string expectedAlg = SecurityAlgorithms.RsaSha256; //Okta uses RS256

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
            catch (SecurityTokenValidationException)
            {
                _log.LogWarning("Security token validation failed.");
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
            //требуется оценка надёжности: процесс аутентификации завязан на поставщика секретов
            string issuer = _config.Get<AppSecrets>().OidcProviderIssuer;
            if (string.IsNullOrEmpty(issuer))
            {
                throw new ApplicationException("Issuer is empty.");
            }

            //значение кешировано большую часть времени (3мс), поэтому проблемы производительности быть не должно
            OpenIdConnectConfiguration discoveryDocument = await _configManager.GetConfigurationAsync(cancellationToken);
            ICollection<SecurityKey> signingKeys = discoveryDocument.SigningKeys;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ////ValidAudience = "",
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            return validationParameters;
        }
    }
}
