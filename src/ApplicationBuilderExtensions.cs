using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;
using YA.TenantWorker.Infrastructure.Logging.Requests;
using Serilog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace YA.TenantWorker
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDeveloperErrorPages(this IApplicationBuilder application)
        {
            return application
                .UseDatabaseErrorPage()
                .UseDeveloperExceptionPage();
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
                .GetRequiredService<CacheProfileOptions>()
                .Where(x => string.Equals(x.Key, CacheProfileName.StaticFiles, StringComparison.Ordinal))
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

        public static IApplicationBuilder UseRouteParamsLogging(this IApplicationBuilder application)
        {
            return application.UseSerilogRequestLogging(
                options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    Endpoint endpoint = httpContext.GetEndpoint();
                    string routeName = endpoint?.Metadata?.GetMetadata<IRouteNameMetadata>()?.RouteName;
                    diagnosticContext.Set("RouteName", routeName);
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                });
        }

        public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder application, AppSecrets secrets)
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

                    options.OAuthClientId(secrets.OauthImplicitClientId);
                    options.OAuthScopeSeparator(" ");
                    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> {
                        { "response_type", secrets.OauthImplicitResponseType },
                        { "scope", secrets.OauthImplicitScope },
                        { "nonce","nonce" }
                    });
                });
        }

        public static IApplicationBuilder UseNetworkContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<NetworkContextLogger>();
        }

        public static IApplicationBuilder UseHttpContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<HttpContextLogger>();
        }

        public static IApplicationBuilder UseAuthenticationContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<AuthenticationContextLogger>();
        }
    }
}
