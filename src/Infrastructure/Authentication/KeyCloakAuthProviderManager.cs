using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;
using YA.UserWorker.Infrastructure.Authentication.Dto;
using YA.UserWorker.Options;

namespace YA.UserWorker.Infrastructure.Authentication;

/// <summary>
/// Сервис управления поставщиком аутентификации KeyCloak.
/// </summary>
public class KeyCloakAuthProviderManager : IAuthProviderManager
{
    public KeyCloakAuthProviderManager(ILogger<KeyCloakAuthProviderManager> logger,
        IRuntimeContextAccessor runtimeCtx,
        IOptions<AppSecrets> secrets,
        IOptions<OauthOptions> oauthOptions,
        IHttpClientFactory httpClientFactory)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
        _secrets = secrets.Value;
        _oauthOptions = oauthOptions.Value;

        string autorityUrl = oauthOptions.Value?.Authority;

        if (string.IsNullOrWhiteSpace(autorityUrl))
        {
            throw new InvalidOperationException("Authority URL is not found.");
        }

        Uri authorityUri = new Uri(autorityUrl);
        //string[] arr = new string[authorityUri.Segments.Length + 1];

        IEnumerable<string> arr = new List<string>();

        foreach (string str in authorityUri.Segments)
        {
            if (str == "realms/")
            {
                arr = arr.Append("admin/");
            }

            arr = arr.Append(str);
        }

        string finalUrl = authorityUri.Scheme + "://" + authorityUri.Host + "/";

        foreach (string str in arr)
        {
            if (str == "/")
            {
                continue;
            }

            finalUrl += str;
        }

        finalUrl += "/users";

        _userManagementUrl = finalUrl;
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    private readonly ILogger<KeyCloakAuthProviderManager> _log;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSecrets _secrets;
    private readonly OauthOptions _oauthOptions;
    private readonly string _userManagementUrl;

    private async Task<string> GetManagementTokenAsync(CancellationToken cancellationToken)
    {
        string clientId = _secrets.KeycloakManagementApiClientId;
        string clientSecret = _secrets.KeycloakManagementApiClientSecret;

        Auth0ApiManagementTokenResponse tokenObject = null;

        using (HttpRequestMessage tokenRequest = new(HttpMethod.Post, _oauthOptions.TokenUrl))
        using (HttpClient client = _httpClientFactory.CreateClient())
        {
            string requestBody = $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}";
            tokenRequest.Content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot get management token. Status: {tokenResponse.StatusCode}");
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
    /// и тип доступа в артибуты указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (sub).</param>
    /// <param name="tenantId">Идентификатор арендатора.</param>
    /// <param name="accessType">Тип доступа пользователя к арендатору.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task SetTenantAsync(string userId, Guid tenantId, YaMembershipAccessType accessType, CancellationToken cancellationToken)
    {
        string managementToken = await GetManagementTokenAsync(cancellationToken);

        using (HttpRequestMessage updateRequest = new(HttpMethod.Put, $"{_userManagementUrl}/{userId}"))
        using (HttpClient client = _httpClientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", managementToken);

            string requestBody = "{\"attributes\": { \"tid\": \"" + tenantId + "\", \"tenantaccesstype\": \"" + accessType + "\" }}";
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
    /// Получить текущий идентификатор арендатора для пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (sub).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<Guid> GetUserTenantAsync(string userId, CancellationToken cancellationToken)
    {
        Guid result = Guid.Empty;

        string managementToken = await GetManagementTokenAsync(cancellationToken);

        using (HttpRequestMessage getRequest = new(HttpMethod.Get, $"{_userManagementUrl}/{userId}"))
        using (HttpClient client = _httpClientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", managementToken);

            HttpResponseMessage getUserResponse = await client.SendAsync(getRequest, cancellationToken);

            if (!getUserResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot get tenant for user {userId}, status code: {getUserResponse.StatusCode}");
            }

            try
            {
                using (Stream responseStream = await getUserResponse.Content.ReadAsStreamAsync(cancellationToken))
                {
                    Auth0User user = await JsonSerializer
                        .DeserializeAsync<Auth0User>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

                    if (user != null)
                    {
                        if (Guid.TryParse(user.app_metadata.Tid, out Guid tid))
                        {
                            result = tid;
                        }
                    }
                    else
                    {
                        _log.LogWarning("No user available.");
                    }

                    _log.LogInformation("Tenant for user {UserID} has been retrieved", userId);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error getting tenant for {UserID}", userId);
            }
        }

        return result;
    }

    /// <summary>
    /// Удалить идентификатор арендатора для пользователя. Убирает идентификатор арендатора
    /// из хранилища app_metadata указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (sub).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task RemoveTenantAsync(string userId, CancellationToken cancellationToken)
    {
        string managementToken = await GetManagementTokenAsync(cancellationToken);

        using (HttpRequestMessage updateRequest = new(HttpMethod.Patch, $"{_userManagementUrl}/{userId}"))
        using (HttpClient client = _httpClientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", managementToken);

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
