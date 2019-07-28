using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Delobytes.AspNetCore;
using Delobytes.AspNetCore.Swagger;
using Delobytes.AspNetCore.Swagger.OperationFilters;
using Delobytes.AspNetCore.Swagger.SchemaFilters;
using YA.TenantWorker.Health;
using YA.TenantWorker.OperationFilters;
using YA.TenantWorker.Options;
using Swashbuckle.AspNetCore.Swagger;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health.Services;
using YA.TenantWorker.Health.System;

namespace YA.TenantWorker
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods which extend ASP.NET Core services.
    /// </summary>
    public static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelationIdFluent(this IServiceCollection services)
        {
            services.AddCorrelationId();

            return services;
        }

        /// <summary>
        /// Configures caching for the application. Registers the <see cref="IDistributedCache"/> and
        /// <see cref="IMemoryCache"/> types with the services collection or IoC container. The
        /// <see cref="IDistributedCache"/> is intended to be used in cloud hosted scenarios where there is a shared
        /// cache, which is shared between multiple instances of the application. Use the <see cref="IMemoryCache"/>
        /// otherwise.
        /// </summary>
        public static IServiceCollection AddCustomCaching(this IServiceCollection services)
        {
            return services
                // Adds IMemoryCache which is a simple in-memory cache.
                .AddMemoryCache()
                // Adds IDistributedCache which is a distributed cache shared between multiple servers. This adds a
                // default implementation of IDistributedCache which is not distributed.
                .AddDistributedMemoryCache();

            // The last one will override any previously registered IDistributedCache service.
            // .AddDistributedRedisCache(options => { ... });
            
            // .AddSqlServerCache(
            //     x =>
            //     {
            //         x.ConnectionString = "Server=.;Database=ASPNET5SessionState;Trusted_Connection=True;";
            //         x.SchemaName = "dbo";
            //         x.TableName = "Sessions";
            //     });
        }

        /// <summary>
        /// Configures the settings by binding the contents of the appsettings.json file to the specified Plain Old CLR
        /// Objects (POCO) and adding <see cref="IOptions{T}"/> objects to the services collection.
        /// </summary>
        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                // ConfigureSingleton registers IOptions<T> and also T as a singleton to the services collection.
                .ConfigureAndValidateSingleton<ApplicationOptions>(configuration)
                .ConfigureAndValidateSingleton<CompressionOptions>(configuration.GetSection(nameof(ApplicationOptions.Compression)))
                .ConfigureAndValidateSingleton<ForwardedHeadersOptions>(configuration.GetSection(nameof(ApplicationOptions.ForwardedHeaders)))
                .ConfigureAndValidateSingleton<CacheProfileOptions>(configuration.GetSection(nameof(ApplicationOptions.CacheProfiles)));
        }

        /// <summary>
        /// Adds dynamic response compression to enable GZIP compression of responses. This is turned off for HTTPS
        /// requests by default to avoid the BREACH security vulnerability.
        /// </summary>
        public static IServiceCollection AddCustomResponseCompression(this IServiceCollection services)
        {
            return services
                .AddResponseCompression(options =>
                    {
                        // Add additional MIME types (other than the built in defaults) to enable GZIP compression for.
                        IEnumerable<string> customMimeTypes = services
                                                  .BuildServiceProvider()
                                                  .GetRequiredService<CompressionOptions>()
                                                  .MimeTypes ?? Enumerable.Empty<string>();
                        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(customMimeTypes);
                    })
                .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        }

        /// <summary>
        /// Add custom routing settings which determines how URL's are generated.
        /// </summary>
        public static IServiceCollection AddCustomRouting(this IServiceCollection services)
        {
            return services.AddRouting(options =>
                {
                    options.LowercaseUrls = true;
                });
        }

        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
        {
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
            services
                .AddHostedService<MessageBusService>()
                .AddSingleton<MessageBusServiceHealthCheck>()
                .AddHealthChecks()
                    .AddGeneralHealthCheck<UptimeHealthCheck>("uptime")
                    .AddGeneralHealthCheck<MessageBusServiceHealthCheck>(General.MessageBusServiceHealthCheckName, HealthStatus.Degraded, new[] { "ready" })
                    .AddMemoryHealthCheck("memory");
                    // Ping is not available on Azure Web Apps
                    //.AddNetworkHealthCheck("network");

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(5);
                options.Predicate = (check) => check.Tags.Contains("ready");
            });

            // The following workaround permits adding an IHealthCheckPublisher 
            // instance to the service container when one or more other hosted 
            // services have already been added to the app. This workaround
            // won't be required with the release of ASP.NET Core 3.0. For more 
            // information, see: https://github.com/aspnet/Extensions/issues/639.
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService),
                typeof(HealthCheckPublisherOptions).Assembly.GetType("Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckPublisherHostedService")));

            services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher>();
            
            return services.AddHealthChecks().Services;
        }

        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            return services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                });
        }

        /// <summary>
        /// Add and configure Swagger services.
        /// </summary>
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
                {
                    Assembly assembly = typeof(Startup).Assembly;
                    string assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                    string assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

                    options.DescribeAllEnumsAsStrings();
                    options.DescribeAllParametersInCamelCase();
                    options.DescribeStringEnumsInCamelCase();
                    options.EnableAnnotations();

                    // Add the XML comment file for this assembly, so its contents can be displayed.
                    options.IncludeXmlCommentsIfExists(assembly);

                    options.OperationFilter<ApiVersionOperationFilter>();
                    options.OperationFilter<CorrelationIdOperationFilter>();
                    options.OperationFilter<ForbiddenResponseOperationFilter>();
                    options.OperationFilter<UnauthorizedResponseOperationFilter>();

                    // Show an example model for JsonPatchDocument<T>.
                    options.SchemaFilter<JsonPatchDocumentSchemaFilter>();

                    IApiVersionDescriptionProvider provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (ApiVersionDescription apiVersionDescription in provider.ApiVersionDescriptions)
                    {
                        Info info = new Info()
                        {
                            Title = assemblyProduct,
                            Description = apiVersionDescription.IsDeprecated
                                ? $"{assemblyDescription} This API version is deprecated."
                                : assemblyDescription,
                            Version = apiVersionDescription.ApiVersion.ToString(),
                        };
                        options.SwaggerDoc(apiVersionDescription.GroupName, info);
                    }
                });
        }
    }
}
