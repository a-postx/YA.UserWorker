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
using YA.TenantWorker.Infrastructure.Logging;

namespace YA.TenantWorker
{
    public static class ApplicationBuilderExtensions
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

        public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder application)
        {
            return application.UseSwaggerUI(options =>
                {
                    // Set the Swagger UI browser document title.
                    options.DocumentTitle = typeof(Startup).Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                    // Set the Swagger UI to render at '/'.
                    ////options.RoutePrefix = string.Empty;
                    options.DisplayRequestDuration();

                    IApiVersionDescriptionProvider provider = application.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

                    foreach (ApiVersionDescription apiVersionDescription in provider.ApiVersionDescriptions.OrderByDescending(x => x.ApiVersion))
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{apiVersionDescription.GroupName}/swagger.json",
                            $"Version {apiVersionDescription.ApiVersion}");
                    }
                });
        }

        public static IApplicationBuilder UseCorrelationIdContextLogging(this IApplicationBuilder application)
        {
            return application
                .UseMiddleware<CorrelationIdContextLogger>();
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
