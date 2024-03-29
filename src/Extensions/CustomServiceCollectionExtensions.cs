using System.IO.Compression;
using System.Reflection;
using CorrelationId.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using GreenPipes;
using MassTransit;
using MassTransit.Audit;
using MassTransit.PrometheusIntegration;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using YA.Common.Constants;
using YA.UserWorker.Constants;
using YA.UserWorker.Infrastructure.Data;
using YA.UserWorker.Infrastructure.Health;
using YA.UserWorker.Infrastructure.Health.Services;
using YA.UserWorker.Infrastructure.Health.System;
using YA.UserWorker.Infrastructure.Messaging;
using YA.UserWorker.Infrastructure.Messaging.Consumers;
using YA.UserWorker.Infrastructure.Messaging.Test;
using YA.UserWorker.OperationFilters;
using YA.UserWorker.Options;
using YA.UserWorker.Options.Validators;

namespace YA.UserWorker.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods which extend ASP.NET Core services.
/// </summary>
internal static class CustomServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет использование идентификатора корреляции.
    /// </summary>
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
    /// Добавляет кеширование в приложение - регистрирует <see cref="IDistributedCache"/> и
    /// <see cref="IMemoryCache"/> с соответствующими сервисами в контейнере.
    /// </summary>
    public static IServiceCollection AddCustomCaching(this IServiceCollection services, AppSecrets secrets)
    {
        services.AddMemoryCache();

        //https://stackexchange.github.io/StackExchange.Redis/Configuration.html
        //https://gist.github.com/JonCole/925630df72be1351b21440625ff2671f#file-redis-bestpractices-stackexchange-redis-md
        ConfigurationOptions config = new ConfigurationOptions()
        {
            ClientName = $"{Program.AppName}-{Node.Id}",
            AbortOnConnectFail = false, //false в строке подключения азуровского редиса
            Password = secrets.DistributedCachePassword,
            KeepAlive = 60,
            DefaultDatabase = 0,
            AsyncTimeout = 3000,
            SyncTimeout = 15000,
            ConnectTimeout = 15000,
            ConnectRetry = 3,
            Ssl = false
            ////SslProtocols = System.Security.Authentication.SslProtocols.Tls12
        };

        config.EndPoints.Add(secrets.DistributedCacheHost, secrets.DistributedCachePort);

        services
            .AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = config;
            });

        ////services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(config));

        return services;
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
    /// Задаёт настройки приложения (привязываются значения из файла appsettings.json и из секретов)
    /// и добавляет объекты <see cref="IOptions{T}"/> в коллекцию сервисов.
    /// </summary>
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<HostOptions>, HostOptionsValidator>();
        services.AddSingleton<IValidateOptions<AwsOptions>, AwsOptionsValidator>();
        services.AddSingleton<IValidateOptions<OauthOptions>, OauthOptionsValidator>();
        services.AddSingleton<IValidateOptions<GeneralOptions>, GeneralOptionsValidator>();
        services.AddSingleton<IValidateOptions<IdempotencyOptions>, IdempotencyControlOptionsValidator>();

        services.AddSingleton<IValidateOptions<UserWorkerSecrets>, UserWorkerSecretsValidator>();
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
            .Configure<GeneralOptions>(configuration.GetSection(nameof(ApplicationOptions.General)), o => o.BindNonPublicProperties = false)
            .Configure<IdempotencyOptions>(configuration.GetSection(nameof(ApplicationOptions.IdempotencyControl)), o => o.BindNonPublicProperties = false);

        services
            .Configure<UserWorkerSecrets>(configuration.GetSection($"{nameof(AppSecrets)}:{nameof(AppSecrets.UserWorker)}"), o => o.BindNonPublicProperties = false)
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
            IdempotencyOptions idempotencyOptions = services.BuildServiceProvider().GetService<IOptions<IdempotencyOptions>>().Value;

            AppSecrets appSecrets = services.BuildServiceProvider().GetService<IOptions<AppSecrets>>().Value;
            UserWorkerSecrets userWorkerSecrets = services.BuildServiceProvider().GetService<IOptions<UserWorkerSecrets>>().Value;
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

    /// <summary>
    /// Добавляет проверки здоровья приложения.
    /// </summary>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, AppSecrets secrets)
    {
        // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        services
            // проверка дублирует встроенную проверку масстранзита
            ////.AddSingleton<MessageBusServiceHealthCheck>()
            .AddHealthChecks()
                //общий статус системы
                .AddGenericHealthCheck<UptimeHealthCheck>("uptime")
                .AddMemoryHealthCheck(HealthCheckNames.Memory)
                //регулярные проверки внешних компонентов
                .AddSqlServer(secrets.UserWorker.ConnectionString, "SELECT 1;", HealthCheckNames.Database, HealthStatus.Unhealthy, new string[] { "ready", "metric" })
                .AddRedis($"{secrets.DistributedCacheHost}:{secrets.DistributedCachePort},password={secrets.DistributedCachePassword}",
                    HealthCheckNames.DistributedCache, HealthStatus.Degraded, new[] { "ready", "metric" }, new TimeSpan(0, 0, 30));
                // проверка дублирует встроенную проверку масстранзита
                ////.AddGenericHealthCheck<MessageBusServiceHealthCheck>(HealthCheckNames.MessageBus, HealthStatus.Degraded, new[] { "ready", "metric" })
                // ICMP недоступен на Azure Web Apps
                ////.AddNetworkHealthCheck("network");

        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Period = TimeSpan.FromSeconds(60);
            options.Timeout = TimeSpan.FromSeconds(30);
            options.Delay = TimeSpan.FromSeconds(15);
            //options.Predicate = (check) => check.Tags.Contains("ready");
        });

        services.AddSingleton<IHealthCheckPublisher, MetricsPublisher>();
        services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher>();
            
        return services;
    }

    /// <summary>
    /// Добавить версионирование АПИ.
    /// </summary>
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
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services
            .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
            .AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Добавляет автоматическую валидацию моделей.
    /// </summary>
    public static IServiceCollection AddCustomModelValidation(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation(fv =>
            {
                fv.DisableDataAnnotationsValidation = true;
            })
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<Startup>()
            .AddFluentValidationRulesToSwagger();

        return services;
    }

    /// <summary>
    /// Добавляет авторизацию на базе политик доступа и удостоверений.
    /// </summary>
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorizationCore(options =>
            {
                options.AddPolicy("MustBeAdministrator", policy =>
                {
                    policy.RequireClaim(YaClaimNames.role, "Administrator");
                });
                options.AddPolicy(YaPolicyNames.Owner, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == YaClaimNames.tenantaccesstype && c.Value == "Owner"));
                });
                options.AddPolicy(YaPolicyNames.Admin, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Owner")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Admin")));
                });
                options.AddPolicy(YaPolicyNames.Writer, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Owner")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Admin")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "ReadWrite")));
                });
                options.AddPolicy(YaPolicyNames.Reader, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Owner")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "Admin")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "ReadWrite")
                            || (c.Type == YaClaimNames.tenantaccesstype && c.Value == "ReadOnly")));
                });
                options.AddPolicy(YaPolicyNames.NonAnonymous, policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.Identity.IsAuthenticated);
                });
            });

        return services;
    }

    /// <summary>
    /// Добавляет основную базу данных приложения.
    /// </summary>
    public static IServiceCollection AddCustomDatabase(this IServiceCollection services, AppSecrets secrets, IWebHostEnvironment webHostEnvironment)
    {
        services
            .AddDbContext<UserWorkerDbContext>(options =>
                options.UseSqlServer(secrets.UserWorker.ConnectionString, sqlOptions =>
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
                cfg.ConnectSendAuditObservers(auditStore, c => c.Exclude(typeof(IUserWorkerTestRequestV1), typeof(IUserWorkerTestResponseV1)));
                cfg.ConnectConsumeAuditObserver(auditStore, c => c.Exclude(typeof(IUserWorkerTestRequestV1), typeof(IUserWorkerTestResponseV1)));

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

                cfg.ReceiveEndpoint(MbQueueNames.TenantInvitationSentQueueName, e =>
                {
                    e.UseConcurrencyLimit(1);
                    e.UseMessageRetry(x =>
                    {
                        x.Handle<OperationCanceledException>();
                        x.Interval(2, 500);
                    });
                    e.UseMbContextFilter();

                    e.ConfigureConsumer<TenantInvitationSentConsumer>(context);
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
