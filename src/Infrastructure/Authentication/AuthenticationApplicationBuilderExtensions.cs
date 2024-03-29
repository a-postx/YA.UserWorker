using Microsoft.AspNetCore.Builder;

namespace YA.UserWorker.Infrastructure.Authentication;

/// <summary>
/// Методы расширения.
/// </summary>
public static class AuthenticationApplicationBuilderExtensions
{
    /// <summary>
    /// Добавляет аутентификацию приложения на базе Auth0 (OAuth2, OpenID Connect и JWT-токены).
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAuth0Authentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseAuthentication();
    }

    /// <summary>
    /// Добавляет аутентификацию приложения на базе KeyCloak (OAuth2, OpenID Connect и JWT-токены).
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseKeyCloakAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseAuthentication();
    }
}

