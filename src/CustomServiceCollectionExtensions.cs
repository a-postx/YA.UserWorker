﻿using CorrelationId.DependencyInjection;
using Delobytes.AspNetCore;
using Delobytes.AspNetCore.Swagger;
using Delobytes.AspNetCore.Swagger.OperationFilters;
using Delobytes.AspNetCore.Swagger.SchemaFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using YA.Common.Constants;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health;
using YA.TenantWorker.Health.Services;
using YA.TenantWorker.Health.System;
using YA.TenantWorker.OperationFilters;
using YA.TenantWorker.Options;

namespace YA.TenantWorker
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods which extend ASP.NET Core services.
    /// </summary>
    internal static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelationIdFluent(this IServiceCollection services)
        {
            services.AddDefaultCorrelationId(options =>
            {
                options.CorrelationIdGenerator = () => "";
                options.AddToLoggingScope = true;
                options.LoggingScopeKey = YaLogKeys.CorrelationId;
                options.EnforceHeader = false;
                options.IgnoreRequestHeader = false;
                options.IncludeInResponse = false;
                options.RequestHeader = General.CorrelationIdHeader;
                options.ResponseHeader = General.CorrelationIdHeader;
                options.UpdateTraceIdentifier = false;
            });

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
        /// Add cross-origin resource sharing (CORS) services and configures named CORS policies. See
        /// https://docs.asp.net/en/latest/security/cors.html
        /// </summary>
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            return services.AddCors(options =>
            {
                // Create named CORS policies here which you can consume using application.UseCors("PolicyName")
                // or a [EnableCors("PolicyName")] attribute on your controller or action.
                options.AddPolicy(
                    CorsPolicyNames.AllowAny,
                    x => x
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                
            });
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
                .ConfigureAndValidateSingleton<CacheProfileOptions>(configuration.GetSection(nameof(ApplicationOptions.CacheProfiles)))
                .ConfigureAndValidateSingleton<KestrelServerOptions>(configuration.GetSection(nameof(ApplicationOptions.Kestrel)));
        }

        /// <summary>
        /// Adds dynamic response compression to enable GZIP compression of responses. This is turned off for HTTPS
        /// requests by default to avoid the BREACH security vulnerability.
        /// </summary>
        public static IServiceCollection AddCustomResponseCompression(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal)
                .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal)
                .AddResponseCompression(options =>
                    {
                        // Add additional MIME types (other than the built in defaults) to enable GZIP compression for.
                        IEnumerable<string> customMimeTypes = configuration
                            .GetSection(nameof(ApplicationOptions.Compression))
                            .Get<CompressionOptions>()
                            ?.MimeTypes ?? Enumerable.Empty<string>();
                        options.MimeTypes = customMimeTypes.Concat(ResponseCompressionDefaults.MimeTypes);

                        options.Providers.Add<BrotliCompressionProvider>();
                        options.Providers.Add<GzipCompressionProvider>();
                    });
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

        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
            services
                .AddSingleton<MessageBusServiceHealthCheck>()
                .AddHealthChecks()
                    //general system status
                    .AddGenericHealthCheck<UptimeHealthCheck>("uptime")
                    .AddMemoryHealthCheck("memory")
                    //system components regular checks
                    .AddSqlServer(configuration.GetValue<string>(nameof(AppSecrets.TenantWorkerConnStr)),
                        "SELECT 1;", General.SqlDatabaseHealthCheckName, HealthStatus.Unhealthy, new string[] { "ready" })
                    .AddGenericHealthCheck<MessageBusServiceHealthCheck>(General.MessageBusServiceHealthCheckName, HealthStatus.Degraded, new[] { "ready" });
                    // Ping is not available on Azure Web Apps
                    //.AddNetworkHealthCheck("network");

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Period = TimeSpan.FromSeconds(60);
                options.Timeout = TimeSpan.FromSeconds(60);
                options.Delay = TimeSpan.FromSeconds(15);
                options.Predicate = (check) => check.Tags.Contains("ready");
            });

            services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher>();
            
            return services.AddHealthChecks().Services;
        }

        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            return services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                })
                .AddVersionedApiExplorer(x => x.GroupNameFormat = "'v'VVV"); // Version format: 'v'major[.minor][-status]
        }

        /// <summary>
        /// Add and configure Swagger services.
        /// </summary>
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, AppSecrets secrets)
        {
            return services.AddSwaggerGen(options =>
            {
                Assembly assembly = typeof(Startup).Assembly;
                string assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                string assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

                options.DescribeAllParametersInCamelCase();
                options.EnableAnnotations();
                options.AddFluentValidationRules();

                // Add the XML comment file for this assembly, so its contents can be displayed.
                options.IncludeXmlCommentsIfExists(assembly);

                options.OperationFilter<ApiVersionOperationFilter>();
                options.OperationFilter<CorrelationIdOperationFilter>(General.CorrelationIdHeader);
                options.OperationFilter<ContentTypeOperationFilter>(true);
                options.OperationFilter<ClaimsOperationFilter>(secrets.SwaggerAuthenticationSchemeName);
                options.OperationFilter<SecurityRequirementsOperationFilter>(true, secrets.SwaggerAuthenticationSchemeName);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = secrets.SwaggerAuthenticationSchemeName
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                if (secrets.SwaggerAuthenticationSchemeName == "oauth2")
                {
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows()
                        {
                            Implicit = new OpenApiOAuthFlow()
                            {
                                AuthorizationUrl = new Uri(secrets.OauthImplicitAuthorizationUrl),
                                TokenUrl = new Uri(secrets.OauthImplicitTokenUrl),
                                Scopes = new Dictionary<string, string>()
                            }
                        }
                    });
                }

                if (secrets.SwaggerAuthenticationSchemeName == "Bearer")
                {
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Заголовок JWT \"Authorization\" используя схему Bearer. Пример: \"Bearer {token}\"",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });
                }

                // Show an example model for JsonPatchDocument<T>.
                options.SchemaFilter<JsonPatchDocumentSchemaFilter>();

                IApiVersionDescriptionProvider provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (ApiVersionDescription apiVersionDescription in provider.ApiVersionDescriptions)
                {
                    OpenApiInfo info = new OpenApiInfo()
                    {
                        Title = assemblyProduct,
                        Description = apiVersionDescription.IsDeprecated
                            ? $"{assemblyDescription} This API version has been deprecated."
                            : assemblyDescription,
                        Version = apiVersionDescription.ApiVersion.ToString(),
                    };
                    options.SwaggerDoc(apiVersionDescription.GroupName, info);
                }
            });
        }
    }
}
