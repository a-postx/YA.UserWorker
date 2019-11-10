using System;
using CorrelationId;
using GreenPipes;
using MassTransit;
using MassTransit.Audit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Health;
using YA.TenantWorker.Infrastructure.Messaging;
using YA.TenantWorker.Infrastructure.Data;
using Serilog;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YA.TenantWorker.Application.ActionFilters;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Infrastructure.Messaging.Test;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Caching;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace YA.TenantWorker
{
    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup : IStartup
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _hostingEnvironment;

        // controller design generator search for this
        private IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, where key value pair settings are stored. See
        /// http://docs.asp.net/en/latest/fundamentals/configuration.html</param>
        /// <param name="hostingEnvironment">The environment the application is running under. This can be Development,
        /// Staging or Production by default. See http://docs.asp.net/en/latest/fundamentals/environments.html</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

            Configuration = configuration;
        }

        /// <summary>
        /// Configures the services to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
        /// </summary>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            KeyVaultSecrets secrets = _config.Get<KeyVaultSecrets>();

            string connectionString = secrets.TenantWorkerConnStr;

            services
                .AddApplicationInsightsTelemetry(_config)
                
                .AddCorrelationIdFluent()
                
                .AddCustomCaching()
                .AddCustomOptions(_config)
                .AddCustomRouting()
                .AddResponseCaching()
                .AddCustomResponseCompression()
                .AddCustomHealthChecks(_config)
                .AddCustomSwagger()
                .AddHttpContextAccessor()

                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                //.AddTransient(x => x
                //    .GetRequiredService<IUrlHelperFactory>()
                //    .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext))
                //.AddScoped<IPagingLinkHelper, PagingLinkHelper>()

                .AddCustomApiVersioning()
                .AddVersionedApiExplorer(x =>
                    {
                        x.GroupNameFormat = "'v'VVV"; // Version format: 'v'major[.minor][-status]
                    });


            //IdentityServer auth
            ////IConfigurationSection section = Configuration.GetSection("SSOConfig");

            ////services.AddIdentityServer(options =>
            ////    {
            ////        options.Events.RaiseErrorEvents = true;
            ////        options.Events.RaiseInformationEvents = true;
            ////        options.Events.RaiseFailureEvents = true;
            ////        options.Events.RaiseSuccessEvents = true;
            ////        options.PublicOrigin = "http://localhost:7453";
            ////        options.IssuerUri = "http://localhost:7453";
            ////    })
            ////    .AddDeveloperSigningCredential()
            ////    .AddInMemoryApiResources(SSOConfig.GetApiResources(section))
            ////    .AddInMemoryClients(SSOConfig.GetClients(section))
            ////    //.AddInMemoryClients(Config.GetClients())
            ////    .AddInMemoryIdentityResources(Config.GetIdentityResources());


            //Google auth
            ////services.AddIdentity<IdentityUser, IdentityRole>();
            ////services.AddAuthentication(options =>
            ////{
            ////    options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
            ////    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            ////})
            ////.AddGoogle(options =>
            ////{
            ////    options.ClientId = "1015973033066-lh7s32vgvs0acfhg7e5u3v8ff9irdk61.apps.googleusercontent.com";
            ////    options.ClientSecret = "AkaCTPmJH4pHbrCqDAtM4RqV";
            ////    options.CallbackPath = "/auth/getcallback";
            ////    //options.CorrelationCookie = new Microsoft.AspNetCore.Http.CookieBuilder { Name = "Google", };
            ////    options.SaveTokens = true;

            ////    options.Events.OnCreatingTicket = ctx =>
            ////    {
            ////        List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

            ////        tokens.Add(new AuthenticationToken()
            ////        {
            ////            Name = "TicketCreated",
            ////            Value = DateTime.UtcNow.ToString()
            ////        });

            ////        ctx.Properties.StoreTokens(tokens);

            ////        return Task.CompletedTask;
            ////    };
            ////});

            ////services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            ////        .AddJwtBearer(options =>
            ////        {
            ////            options.RequireHttpsMetadata = false;
            ////            options.TokenValidationParameters = new TokenValidationParameters
            ////            {
            ////                NameClaimType = JwtClaimTypes.Name,
            ////                RoleClaimType = JwtClaimTypes.Role,
            ////                ValidateIssuer = true,
            ////                ValidIssuer = "https://localhost:7453",
            ////                ValidateAudience = true,
            ////                ValidAudience = "YATenantWorker",
            ////                ValidateLifetime = true,
            ////                ValidateIssuerSigningKey = true,
            ////                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RANDOM_KEY_MUST_NOT_BE_SHARED")),
            ////            };
            ////        });

            services
                .AddMvcCore()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddApiExplorer()
                    .AddAuthorization()
                    .AddDataAnnotations()
                    .AddJsonFormatters()
                    .AddCustomJsonOptions(_hostingEnvironment)
                    .AddCustomCors()
                    .AddCustomMvcOptions(_hostingEnvironment);
            services
                .AddProjectCommands()
                .AddProjectMappers()
                .AddProjectRepositories()
                .AddProjectServices(secrets);

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<TenantWorkerDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlOptions => 
                        sqlOptions.EnableRetryOnFailure().CommandTimeout(60))
                    .ConfigureWarnings(x => x.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .EnableSensitiveDataLogging(_hostingEnvironment.IsDevelopment()));
                    //// useful for API-related projects that only read data
                    //// we don't need query tracking if dbcontext is disposed on every request
                    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddScoped<IMessageBus, MessageBus>();

            services.AddScoped<TestRequestConsumer>();
            
            services.AddMassTransit(options =>
            {
                options.AddConsumers(GetType().Assembly);
            });

            services.AddSingleton(provider =>
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    IRabbitMqHost host = cfg.Host(secrets.MessageBusHost, secrets.MessageBusVHost, h =>
                    {
                        h.Username(secrets.MessageBusLogin);
                        h.Password(secrets.MessageBusPassword);
                    });

                    cfg.UseSerilog();

                    cfg.ReceiveEndpoint(host, MbQueueNames.PrivateServiceQueueName, e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(x => x.Interval(2, 500));
                        e.AutoDelete = true;
                        e.Durable = false;
                        e.ExchangeType = "fanout";
                        e.Exclusive = true;
                        e.ExclusiveConsumer = true;
                        ////e.SetExchangeArgument("x-delayed-type", "direct");

                        e.Consumer<TestRequestConsumer>(provider);
                    });
                });
            });

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            //services.AddScoped(provider => provider.GetRequiredService<IBus>().CreatePublishRequestClient<ICreateTenantV1, ITenantCreatedV1>(TimeSpan.FromSeconds(5)));
            
            services.AddSingleton<IMessageAuditStore, MessageAuditStore>();
            
            services.AddScoped<TenantRouteFilter>();
            services.AddScoped<ApiRequestFilter>();
            services.AddScoped<LoggingFilter>();

            services.AddTransient<IApiRequestManager, ApiRequestManager>();
            services.AddSingleton<ApiRequestMemoryCache>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is
        /// called by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            application

                // Pass a GUID in X-Correlation-ID HTTP header to set the HttpContext.TraceIdentifier.
                // UpdateTraceIdentifier must be false due to a bug. See https://github.com/aspnet/AspNetCore/issues/5144
                .UseCorrelationId(new CorrelationIdOptions {
                    Header = General.CorrelationIdHeader,
                    IncludeInResponse = false,
                    UpdateTraceIdentifier = false,
                    UseGuidForCorrelationId = false
                    })
                
                //!experimental!
                ////.UseHttpException()

                .UseForwardedHeaders()
                .UseResponseCaching()
                .UseResponseCompression()
                
                .UseMiddleware<HttpRequestLogger>()

                ////.UseIf(
                ////    !_hostingEnvironment.IsDevelopment(),
                ////    x => x.UseHsts())

                ////.UseIf(
                ////    _hostingEnvironment.IsDevelopment(),
                ////    x => x.UseDeveloperErrorPages())

                .UseHealthChecks("/status", new HealthCheckOptions()
                {
                    ResponseWriter = HealthResponse.WriteResponse
                })

                // The readiness check uses all registered checks with the 'ready' tag.
                .UseHealthChecks("/status/ready", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                    ResponseWriter = HealthResponse.WriteResponse
                })

                .UseHealthChecks("/status/live", new HealthCheckOptions()
                {
                    // Exclude all checks and return a 200-Ok.
                    Predicate = (_) => false,
                    ResponseWriter = HealthResponse.WriteResponse
                })

                .UseStaticFilesWithCacheControl()

                //.UseIdentityServer()
                //////////////////.UseHttpsRedirection()
                ////.UseAuthentication()
                //.UseAuthorization()

                .UseMvc()
                .UseSwagger()
                .UseCustomSwaggerUI();

            //automigration - dangerous, use SQL scripts instead
            using (IServiceScope scope = application.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                TenantWorkerDbContext dbContext = scope.ServiceProvider.GetService<TenantWorkerDbContext>();

                if (dbContext.Database.GetPendingMigrations().GetEnumerator().MoveNext())
                {
                    dbContext.Database.Migrate();
                }
            }
        }
    }
}