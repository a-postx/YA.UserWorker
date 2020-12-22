using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YA.Common.Constants;
using YA.TenantWorker.Application.Middlewares;
using YA.TenantWorker.Infrastructure.Logging.Requests;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDeveloperErrorPages(this IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {
            return application
                .UseIf(
                    webHostEnvironment.EnvironmentName == "Development",
                    x => x
                        .UseDeveloperExceptionPage()
                        .UseMigrationsEndPoint()
                        .UseDeveloperExceptionPage());
        }

        /// <summary>
        /// Uses the static files middleware to serve static files. Also adds the Cache-Control and Pragma HTTP
        /// headers. The cache duration is controlled from configuration.
        /// See http://andrewlock.net/adding-cache-control-headers-to-static-files-in-asp-net-core/.
        /// </summary>
        public static IApplicationBuilder UseStaticFilesWithCacheControl(this IApplicationBuilder application)
        {
            CacheProfile cacheProfile = application
                .ApplicationServices
                .GetRequiredService<IOptions<CacheProfileOptions>>().Value
                .Where(x => string.Equals(x.Key, CacheProfileNames.StaticFiles, StringComparison.Ordinal))
                .Select(x => x.Value)
                .SingleOrDefault();

            application.UseStaticFiles(
                new StaticFileOptions()
                {
                    OnPrepareResponse = context =>
                    {
                        context.Context.ApplyCacheProfile(cacheProfile);
                    },
                });

            return application;
        }

        /// <summary>
        /// Добавляет прослойку для логирования HTTP-запросов встроенными средствами Серилог
        /// </summary>
        public static IApplicationBuilder UseRouteParamsLogging(this IApplicationBuilder application)
        {
            return application.UseSerilogRequestLogging(
                options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    Endpoint endpoint = httpContext.GetEndpoint();
                    string routeName = endpoint?.Metadata?.GetMetadata<IRouteNameMetadata>()?.RouteName;
                    diagnosticContext.Set("RouteName", routeName);
                });
        }

        /// <summary>
        /// Добавляет прослойку кастомизированного пользовательского интерфейса Свагер
        /// </summary>
        public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder application, OauthOptions oauthOptions)
        {
            return application.UseSwaggerUI(options =>
            {
                // Set the Swagger UI browser document title.
                options.DocumentTitle = typeof(Startup).Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                // Set the Swagger UI to render at '/'.
                ////options.RoutePrefix = string.Empty;

                options.DisplayOperationId();
                options.DisplayRequestDuration();

                IApiVersionDescriptionProvider provider = application.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

                foreach (ApiVersionDescription apiVersionDescription in provider.ApiVersionDescriptions.OrderByDescending(x => x.ApiVersion))
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{apiVersionDescription.GroupName}/swagger.json",
                        $"Version {apiVersionDescription.ApiVersion}");
                }

                options.OAuthClientId(oauthOptions.ClientId);
                options.OAuthScopeSeparator(" ");
                options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> {
                        { "scope", "openid profile email" },
                        { "nonce","nonce" }
                    });
            });
        }

        /// <summary>
        /// Добавляет прослойку логирования сетевого контекста
        /// </summary>
        public static IApplicationBuilder UseNetworkContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<NetworkContextLogger>();
        }

        /// <summary>
        /// Добавляет прослойку логирования контекста HTTP
        /// </summary>
        public static IApplicationBuilder UseHttpContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<HttpContextLogger>();
        }

        /// <summary>
        /// Добавляет прослойку перехвата исключения в HTTP-контексте. Выводит детали проблемы с дополнительным контекстом.
        /// Внимание! При отключении этой прослойки все необработанные исключения станут уходить в контекст MVC,
        /// перестанут выдавать <see cref="ProblemDetails"/> и логироваться в событии завершения запроса.
        /// </summary>
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<HttpExceptionHandler>();
        }

        /// <summary>
        /// Добавляет прослойку логирования аутентификационного контекста
        /// </summary>
        public static IApplicationBuilder UseAuthenticationContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<AuthenticationContextLogger>();
        }

        /// <summary>
        /// Добавляет прослойку логирования идентификатора клиентского запроса
        /// </summary>
        public static IApplicationBuilder UseClientRequestContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<ClientRequestContextLogger>();
        }
    }
}
