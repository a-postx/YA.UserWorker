﻿using System;
using CorrelationId;
using GreenPipes;
using MassTransit;
using MassTransit.Audit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.ActionFilters;
using YA.TenantWorker.Infrastructure.Messaging;
using YA.TenantWorker.Infrastructure.Data;
using YA.TenantWorker.Infrastructure.Messaging.Test;
using YA.TenantWorker.Infrastructure.Authentication;
using YA.TenantWorker.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using YA.TenantWorker.Infrastructure.Logging.MbMessages;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using YA.TenantWorker.Application;
using YA.TenantWorker.Core.Entities;
using System.Text;
using YA.TenantWorker.Infrastructure.Messaging.Consumers;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace YA.TenantWorker
{
    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // controller design generator search for this
        private IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, where key value pair settings are stored. See
        /// http://docs.asp.net/en/latest/fundamentals/configuration.html</param>
        /// <param name="webHostEnvironment">The environment the application is running under. This can be Development,
        /// Staging or Production by default. See http://docs.asp.net/en/latest/fundamentals/environments.html</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

            Configuration = configuration;
        }

        /// <summary>
        /// Configures the services to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            AWSOptions awsOptions = _config.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

            services.Configure<HostOptions>(option =>
            {
                option.ShutdownTimeout = TimeSpan.FromSeconds(General.SystemShutdownTimeoutSec);
            });

            AppSecrets secrets = _config.Get<AppSecrets>();

            if (!string.IsNullOrEmpty(secrets.AppInsightsInstrumentationKey))
            {
                ApplicationInsightsServiceOptions options = new ApplicationInsightsServiceOptions
                {
                    DeveloperMode = _webHostEnvironment.IsDevelopment(),
                    InstrumentationKey = secrets.AppInsightsInstrumentationKey
                };

                services.AddApplicationInsightsTelemetry(options);
            }
            
            services
                .AddCorrelationIdFluent()

                ////.AddHttpsRedirection(options =>
                ////{
                ////    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                ////})

                .AddCustomCaching()
                .AddCustomCors()
                .AddCustomOptions(_config)
                .AddCustomRouting()
                .AddResponseCaching()
                .AddCustomResponseCompression(_config)
                .AddCustomHealthChecks(_config)
                .AddCustomSwagger()
                .AddHttpContextAccessor()

                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()

                .AddCustomApiVersioning();

            services.AddAuthenticationCore(o =>
            {
                o.DefaultScheme = "YaHeaderClaimsScheme";
                o.AddScheme<YaAuthenticationHandler>("YaHeaderClaimsScheme", "Authentication scheme that use claims extracted from JWT token.");
            });

            services
                .AddControllers()
                    .AddCustomJsonOptions(_webHostEnvironment)
                    ////.AddXmlDataContractSerializerFormatters()
                    .AddCustomMvcOptions(_config);

            services
                .AddAuthorizationCore(options => options.AddPolicy("MustBeAdministrator", policy => policy.RequireClaim(CustomClaimNames.role, "Administrator")));

            services.AddAutoMapper(typeof(Startup));

            services
                .AddProjectCommands()
                .AddProjectComponents()
                .AddProjectMappers()
                .AddProjectRepositories()
                .AddProjectServices();

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<TenantWorkerDbContext>(options =>
                    options.UseSqlServer(secrets.TenantWorkerConnStr, sqlOptions => 
                        sqlOptions.EnableRetryOnFailure().CommandTimeout(60))
                    .ConfigureWarnings(x => x.Throw(RelationalEventId.QueryPossibleExceptionWithAggregateOperatorWarning))
                    .EnableSensitiveDataLogging(_webHostEnvironment.IsDevelopment()));
                    //// useful for API-related projects that only read data
                    //// we don't need query tracking if dbcontext is disposed on every request
                    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddScoped<IMessageBus, MessageBus>();

            services.AddScoped<TestRequestConsumer>();
            services.AddScoped<GetPricingTierConsumer>();

            services.AddMassTransit(options =>
            {
                options.AddConsumers(GetType().Assembly);
            });

            services.AddSingleton(provider =>
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(secrets.MessageBusHost, secrets.MessageBusVHost, h =>
                    {
                        h.Username(secrets.MessageBusLogin);
                        h.Password(secrets.MessageBusPassword);
                    });

                    cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
                    cfg.UseSerilogMessagePropertiesEnricher();

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
                        e.Exclusive = true;
                        e.ExclusiveConsumer = true;
                        ////e.SetExchangeArgument("x-delayed-type", "direct");

                        e.Consumer<TestRequestConsumer>(provider);
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

                        e.Consumer<GetPricingTierConsumer>(provider);
                    });
                });
            });

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            //services.AddScoped(provider => provider.GetRequiredService<IBus>().CreatePublishRequestClient<ICreateTenantV1, ITenantCreatedV1>(TimeSpan.FromSeconds(5)));
            
            services.AddSingleton<IMessageAuditStore, MessageAuditStore>();
            
            services.AddScoped<ApiRequestFilter>();

            services.AddScoped<IApiRequestTracker, ApiRequestTracker>();
            services.AddSingleton<IApiRequestMemoryCache, ApiRequestMemoryCache>();
        }

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after IHost Run() by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            application
                // Pass a GUID in X-Correlation-ID HTTP header to set the HttpContext.TraceIdentifier.
                .UseCorrelationId(new CorrelationIdOptions {
                    Header = General.CorrelationIdHeader,
                    IncludeInResponse = false,
                    UpdateTraceIdentifier = true,
                    UseGuidForCorrelationId = false
                    })
                .UseCorrelationIdContextLogging()

                ////.UseHttpsRedirection()

                ////.UseAllElasticApm(Configuration)

                //!experimental!
                ////.UseHttpException()

                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                })
                .UseNetworkContextLogging()

                .UseResponseCaching()
                .UseResponseCompression()
                
                .UseHttpContextLogging()
                
                .UseRouting()
                .UseCors(CorsPolicyName.AllowAny)
                .UseStaticFilesWithCacheControl()
                .UseRouteParamsLogging()

                ////.UseIf(
                ////    !_webHostEnvironment.IsDevelopment(),
                ////    x => x.UseHsts())

                ////.UseIf(_webHostEnvironment.IsDevelopment(),
                ////    x => x.UseDeveloperErrorPages())

                ////.UseHttpsRedirection()
                .UseAuthentication()
                .UseAuthenticationContextLogging()
                .UseAuthorization()

                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers().RequireCors(CorsPolicyName.AllowAny);
                    endpoints.MapHealthChecks("/status", new HealthCheckOptions()
                    {
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyName.AllowAny);
                    endpoints.MapHealthChecks("/status/ready", new HealthCheckOptions()
                    {
                        Predicate = (check) => check.Tags.Contains("ready"),
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyName.AllowAny);
                    endpoints.MapHealthChecks("/status/live", new HealthCheckOptions()
                    {
                        // Exclude all checks and return a 200-Ok.
                        Predicate = (_) => false,
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyName.AllowAny);
                    endpoints.MapGet("/nodeid", async (context) =>
                    {
                        await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(Node.Id));
                    }).RequireCors(CorsPolicyName.AllowAny);
                })

                .UseSwagger()
                .UseCustomSwaggerUI();
        }
    }
}