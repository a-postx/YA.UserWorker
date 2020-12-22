using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using CorrelationId.DependencyInjection;
using Delobytes.AspNetCore.Swagger;
using Delobytes.AspNetCore.Swagger.OperationFilters;
using Delobytes.AspNetCore.Swagger.SchemaFilters;
using GreenPipes;
using MassTransit;
using MassTransit.Audit;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using YA.Common.Constants;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health;
using YA.TenantWorker.Health.Services;
using YA.TenantWorker.Health.System;
using YA.TenantWorker.Infrastructure.Data;
using YA.TenantWorker.Infrastructure.Messaging;
using YA.TenantWorker.Infrastructure.Messaging.Consumers;
using YA.TenantWorker.Infrastructure.Messaging.Test;
using YA.TenantWorker.OperationFilters;
using YA.TenantWorker.Options;
using YA.TenantWorker.Options.Validators;

namespace YA.TenantWorker.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods which extend ASP.NET Core services.
    /// </summary>
    internal static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelationIdFluent(this IServiceCollection services, GeneralOptions generalOptions)
        {
            services.AddDefaultCorrelationId(options =>
            {
                options.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
                options.AddToLoggingScope = true;
                options.LoggingScopeKey = YaLogKeys.CorrelationId;
                options.EnforceHeader = false;
                options.IgnoreRequestHeader = false;
                options.IncludeInResponse = true;
                options.RequestHeader = generalOptions.CorrelationIdHeader;
                options.ResponseHeader = generalOptions.CorrelationIdHeader;
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
            services.AddSingleton<IValidateOptions<HostOptions>, HostOptionsValidator>();
            services.AddSingleton<IValidateOptions<AwsOptions>, AwsOptionsValidator>();
            services.AddSingleton<IValidateOptions<OauthOptions>, OauthOptionsValidator>();
            services.AddSingleton<IValidateOptions<GeneralOptions>, GeneralOptionsValidator>();

            services.AddSingleton<IValidateOptions<TenantWorkerSecrets>, TenantWorkerSecretsValidator>();
            services.AddSingleton<IValidateOptions<AppSecrets>, AppSecretsValidator>();

            services
                .Configure<ApplicationOptions>(configuration, o => o.BindNonPublicProperties = false)
                .Configure<HostOptions>(configuration.GetSection(nameof(ApplicationOptions.HostOptions)), o => o.BindNonPublicProperties = false)
                .Configure<AwsOptions>(configuration.GetSection(nameof(ApplicationOptions.Aws)), o => o.BindNonPublicProperties = false)
                .Configure<CompressionOptions>(configuration.GetSection(nameof(ApplicationOptions.Compression)), o => o.BindNonPublicProperties = false)
                .Configure<ForwardedHeadersOptions>(configuration.GetSection(nameof(ApplicationOptions.ForwardedHeaders)), o => o.BindNonPublicProperties = false)
                .Configure<CacheProfileOptions>(configuration.GetSection(nameof(ApplicationOptions.CacheProfiles)), o => o.BindNonPublicProperties = false)
                .Configure<KestrelServerOptions>(configuration.GetSection(nameof(ApplicationOptions.Kestrel)), o => o.BindNonPublicProperties = false)
                .Configure<OauthOptions>(configuration.GetSection(nameof(ApplicationOptions.OAuth)), o => o.BindNonPublicProperties = false)
                .Configure<GeneralOptions>(configuration.GetSection(nameof(ApplicationOptions.General)), o => o.BindNonPublicProperties = false);

            services
                .Configure<TenantWorkerSecrets>(configuration.GetSection($"{nameof(AppSecrets)}:{nameof(AppSecrets.TenantWorker)}"), o => o.BindNonPublicProperties = false)
                .Configure<AppSecrets>(configuration.GetSection(nameof(AppSecrets)), o => o.BindNonPublicProperties = false);

            return services;
        }

        /// <summary>
        /// Создаёт экземпляры всех настроек и получает значения, чтобы провести процесс валидации при старте приложения.
        /// </summary>
        public static IServiceCollection AddOptionsAndSecretsValidationOnStartup(this IServiceCollection services)
        {
            try
            {
                HostOptions hostOptions = services.BuildServiceProvider().GetService<IOptions<HostOptions>>().Value;
                AwsOptions awsOptions = services.BuildServiceProvider().GetService<IOptions<AwsOptions>>().Value;
                ApplicationOptions applicationOptions = services.BuildServiceProvider().GetService<IOptions<ApplicationOptions>>().Value;
                OauthOptions oauthOptions = services.BuildServiceProvider().GetService<IOptions<OauthOptions>>().Value;
                GeneralOptions generalOptions = services.BuildServiceProvider().GetService<IOptions<GeneralOptions>>().Value;

                AppSecrets appSecrets = services.BuildServiceProvider().GetService<IOptions<AppSecrets>>().Value;
                TenantWorkerSecrets tenantWorkerSecrets = services.BuildServiceProvider().GetService<IOptions<TenantWorkerSecrets>>().Value;
            }
            catch (OptionsValidationException ex)
            {
                Console.WriteLine($"Error validating {ex.OptionsType.FullName}: {string.Join(", ", ex.Failures)}");
                throw;
            }

            return services;
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

        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, AppSecrets secrets)
        {
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
            services
                .AddSingleton<MessageBusServiceHealthCheck>()
                .AddHealthChecks()
                    //general system status
                    .AddGenericHealthCheck<UptimeHealthCheck>("uptime")
                    .AddMemoryHealthCheck("memory")
                    //system components regular checks
                    .AddSqlServer(secrets.TenantWorker.ConnectionString, "SELECT 1;", "sql_database", HealthStatus.Unhealthy, new string[] { "ready" })
                    .AddGenericHealthCheck<MessageBusServiceHealthCheck>("message_bus_service", HealthStatus.Degraded, new[] { "ready" });
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
            
            return services;
        }

        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            return services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                })
                .AddVersionedApiExplorer(x => x.GroupNameFormat = "'v'VVV"); // Version format: 'v'major[.minor][-status]
        }

        /// <summary>
        /// Add and configure Swagger services.
        /// </summary>
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, AppSecrets secrets, GeneralOptions generalOptions)
        {
            string swaggerAuthenticationSchemeName = "oauth2";

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
                options.OperationFilter<ClientRequestIdOperationFilter>(generalOptions.ClientRequestIdHeader);
                options.OperationFilter<ContentTypeOperationFilter>(true);
                options.OperationFilter<ClaimsOperationFilter>(swaggerAuthenticationSchemeName);
                options.OperationFilter<SecurityRequirementsOperationFilter>(true, swaggerAuthenticationSchemeName);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = swaggerAuthenticationSchemeName
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                if (swaggerAuthenticationSchemeName == "oauth2")
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

                if (swaggerAuthenticationSchemeName == "Bearer")
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
                        Version = apiVersionDescription.ApiVersion.ToString()
                    };
                    options.SwaggerDoc(apiVersionDescription.GroupName, info);
                }
            });
        }

        /// <summary>
        /// Добавляет кастомизированную базу данных.
        /// </summary>
        public static IServiceCollection AddCustomDatabase(this IServiceCollection services, AppSecrets secrets, IWebHostEnvironment webHostEnvironment)
        {
            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<TenantWorkerDbContext>(options =>
                    options.UseSqlServer(secrets.TenantWorker.ConnectionString, sqlOptions =>
                        sqlOptions.EnableRetryOnFailure().CommandTimeout(Timeouts.SqlCommandTimeoutSec))
                    .ConfigureWarnings(warnings =>
                    {
                        warnings.Log(RelationalEventId.TransactionError);

                        warnings.Ignore(CoreEventId.ContextInitialized);

                        if (webHostEnvironment.IsProduction())
                        {
                            warnings.Ignore(RelationalEventId.CommandExecuted);
                            warnings.Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning);
                        }

                        if (webHostEnvironment.IsDevelopment())
                        {
                            warnings.Log(RelationalEventId.CommandExecuted);
                            warnings.Throw(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning);
                        }

                        warnings.Throw(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables);
                    })
                    .EnableSensitiveDataLogging(webHostEnvironment.IsDevelopment()))
                .AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }

        /// <summary>
        /// Добавляет кастомизированную шину сообщений.
        /// </summary>
        public static IServiceCollection AddCustomMessageBus(this IServiceCollection services, AppSecrets secrets)
        {
            services.AddSingleton<IMessageAuditStore, MessageAuditStore>();

            services.AddMassTransit(options =>
            {
                options.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(secrets.MessageBusHost, secrets.MessageBusVHost, h =>
                    {
                        h.Username(secrets.MessageBusLogin);
                        h.Password(secrets.MessageBusPassword);
                    });

                    IMessageAuditStore auditStore = context.GetRequiredService<IMessageAuditStore>();
                    cfg.ConnectSendAuditObservers(auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));
                    cfg.ConnectConsumeAuditObserver(auditStore, c => c.Exclude(typeof(ITenantWorkerTestRequestV1), typeof(ITenantWorkerTestResponseV1)));

                    cfg.UseHealthCheck(context);

                    cfg.UseSerilogMessagePropertiesEnricher();
                    cfg.UsePrometheusMetrics();

                    cfg.ReceiveEndpoint(MbQueueNames.PrivateServiceQueueName, e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(x =>
                        {
                            x.Handle<OperationCanceledException>();
                            x.Interval(2, 500);
                        });
                        e.AutoDelete = true;
                        e.Durable = false;
                        e.ExchangeType = "fanout";
                        e.Exclusive = false;
                        e.ExclusiveConsumer = false;
                        ////e.SetExchangeArgument("x-delayed-type", "direct");

                        e.ConfigureConsumer<TestRequestConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(MbQueueNames.PricingTierQueueName, e =>
                    {
                        e.UseConcurrencyLimit(1);
                        e.UseMessageRetry(x =>
                        {
                            x.Handle<OperationCanceledException>();
                            x.Interval(2, 500);
                        });
                        e.UseMbContextFilter();
                        e.UseInMemoryOutbox();

                        e.ConfigureConsumer<GetPricingTierConsumer>(context);
                    });
                });

                options.AddConsumers(Assembly.GetExecutingAssembly());
            });

            services.AddMassTransitHostedService();

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            return services;
        }
    }
}
