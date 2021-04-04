using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Infrastructure.Authentication.Dto;
using YA.UserWorker.Options;

namespace YA.UserWorker.Infrastructure.Authentication
{
    /// <summary>
    /// Сервис управления поставщиком аутентификации Auth0. Ограничение - 1000 запросов в месяц.
    /// </summary>
    public class Auth0AuthProviderManager : IAuthProviderManager
    {
        public Auth0AuthProviderManager(ILogger<Auth0AuthProviderManager> logger,
            IRuntimeContextAccessor runtimeCtx,
            IOptions<AppSecrets> secrets,
            IHttpClientFactory httpClientFactory)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
            _secrets = secrets.Value;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        private readonly ILogger<Auth0AuthProviderManager> _log;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSecrets _secrets;

        private async Task<string> GetManagementTokenAsync(CancellationToken cancellationToken)
        {
            string clientId = _secrets.OauthManagementApiClientId;
            string clientSecret = _secrets.OauthManagementApiClientSecret;
            string audience = _secrets.OauthManagementApiUrl + "/";

            Auth0ApiManagementTokenResponse tokenObject = null;

            using (HttpRequestMessage tokenRequest = new(HttpMethod.Post, _secrets.OauthImplicitTokenUrl))
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                string requestBody = $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&audience={audience}";

                tokenRequest.Content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Cannot get management token.");
                }

                string tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                tokenObject = JsonSerializer
                    .Deserialize<Auth0ApiManagementTokenResponse>(tokenContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (!string.IsNullOrEmpty(tokenObject.Access_token))
            {
                return tokenObject.Access_token;
            }
            else
            {
                throw new InvalidOperationException("Management token is empty.");
            }
        }

        /// <summary>
        /// Установить идентификатор арендатора для пользователя. Добавляет идентификатор арендатора
        /// в хранилище app_metadata указанного пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя (sub).</param>
        /// <param name="tenantId">Идентификатор арендатора.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task SetTenantIdAsync(string userId, Guid tenantId, CancellationToken cancellationToken)
        {
            string managementToken = await GetManagementTokenAsync(cancellationToken);

            using (HttpRequestMessage updateRequest = new(HttpMethod.Patch, $"{_secrets.OauthManagementApiUrl}/users/{userId}"))
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", managementToken);
                string correlationId = _runtimeCtx.GetCorrelationId().ToString();
                client.DefaultRequestHeaders.Add("x-correlation-id", correlationId);

                string requestBody = "{\"app_metadata\": { \"tid\": \"" + tenantId + "\" }}";
                updateRequest.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = await client.SendAsync(updateRequest, cancellationToken);

                if (!updateResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Cannot update tenant ID for user {userId}, status code: {updateResponse.StatusCode}");
                }

                _log.LogInformation("{UserId} has been updated with tenant {TenantId}", userId, tenantId);
            }
        }

        /// <summary>
        /// Удалить идентификатор арендатора для пользователя. Убирает идентификатор арендатора
        /// из хранилища app_metadata указанного пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя (sub).</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task RemoveTenantIdAsync(string userId, CancellationToken cancellationToken)
        {
            string managementToken = await GetManagementTokenAsync(cancellationToken);

            using (HttpRequestMessage updateRequest = new(HttpMethod.Patch, $"{_secrets.OauthManagementApiUrl}/users/{userId}"))
            using (HttpClient client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", managementToken);
                string correlationId = _runtimeCtx.GetCorrelationId().ToString();
                client.DefaultRequestHeaders.Add("x-correlation-id", correlationId);

                string requestBody = "{\"app_metadata\": {  }}";
                updateRequest.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = await client.SendAsync(updateRequest, cancellationToken);

                if (!updateResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Cannot remove tenant from user {userId}, status code: {updateResponse.StatusCode}");
                }

                _log.LogInformation("Tenant has been removed from user {UserID}", userId);
            }
        }
    }
}
