using System;
using System.Reflection;
using System.Text;
using Amazon.Extensions.NETCore.Setup;
using AutoMapper;
using CorrelationId;
using MassTransit;
using MediatR;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Prometheus;
using YA.Common.Constants;
using YA.TenantWorker.Application;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Middlewares.ActionFilters;
using YA.TenantWorker.Extensions;
using YA.TenantWorker.Health;
using YA.TenantWorker.Infrastructure.Authentication;
using YA.TenantWorker.Infrastructure.Caching;
using YA.TenantWorker.Options;
//using Elastic.Apm.NetCoreAll;

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
            services
                .AddCustomOptions(_config)
                .AddOptionsAndSecretsValidationOnStartup();

            AppSecrets secrets = _config.GetSection(nameof(AppSecrets)).Get<AppSecrets>();
            GeneralOptions generalOptions = _config.GetSection(nameof(ApplicationOptions.General)).Get<GeneralOptions>();

            AWSOptions awsOptions = _config.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

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
                .AddCorrelationIdFluent(generalOptions)

                .AddCustomCaching()
                .AddCustomCors()
                .AddCustomRouting()
                .AddResponseCaching()
                .AddCustomResponseCompression(_config)
                .AddCustomHealthChecks(secrets)
                .AddCustomSwagger(secrets, generalOptions)
                .AddHttpContextAccessor()

                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()

                .AddCustomApiVersioning();

            //забираем регулярно обновляемые ключи шифрования токенов с сервера провайдера
            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(provider =>
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    secrets.OidcProviderIssuer + ".well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever { RequireHttps = true })
            );

            services.AddAuthenticationCore(o =>
            {
                o.DefaultScheme = "YaScheme";
                o.AddScheme<YaAuthenticationHandler>("YaScheme", "Authentication scheme that use claims extracted from JWT token.");
            });

            services
                .AddControllers()
                    .AddCustomJsonOptions(_webHostEnvironment)
                    ////.AddXmlDataContractSerializerFormatters()
                    .AddCustomMvcOptions(_config)
                    .AddCustomModelValidation();

            services.AddCustomProblemDetails();

            services
                .AddAuthorizationCore(options =>
                {
                    options.AddPolicy("MustBeAdministrator", policy =>
                    {
                        policy.RequireClaim(YaClaimNames.role, "Administrator");
                    });
                });

            services.AddHttpClient();
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services
                .AddProjectActionHandlers()
                .AddProjectMappers()
                .AddProjectRepositories()
                .AddProjectServices();

            services.AddCustomDatabase(secrets, _webHostEnvironment);

            services.AddCustomMessageBus(secrets);

            services.AddScoped<ApiRequestFilter>();
            services.AddScoped<IApiRequestTracker, ApiRequestTracker>();
            services.AddSingleton<IApiRequestMemoryCache, ApiRequestMemoryCache>();
        }

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after IHost Run() by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            OauthOptions oauthOptions = _config.GetSection(nameof(ApplicationOptions.OAuth)).Get<OauthOptions>();

            application
                .UseClientRequestContextLogging()
                .UseCorrelationId()

                //.UseAllElasticApm(Configuration)

                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
                })
                .UseNetworkContextLogging()

                .UseResponseCaching()
                .UseResponseCompression()

                .UseHttpContextLogging()
                .UseCustomExceptionHandler()

                .UseRouting()
                .UseCors(CorsPolicyNames.AllowAny)
                .UseStaticFilesWithCacheControl()
                //временно убираем для сокращения объёма журналов
                ////.UseRouteParamsLogging()

                .UseHealthChecksPrometheusExporter("/metrics")
                .UseMetricServer()
                .UseHttpMetrics()

                .UseAuthentication()
                .UseAuthenticationContextLogging()
                .UseAuthorization()

                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers().RequireCors(CorsPolicyNames.AllowAny);
                    endpoints.MapHealthChecks("/status", new HealthCheckOptions()
                    {
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyNames.AllowAny);
                    endpoints.MapHealthChecks("/status/ready", new HealthCheckOptions()
                    {
                        Predicate = (check) => check.Tags.Contains("ready"),
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyNames.AllowAny);
                    endpoints.MapHealthChecks("/status/live", new HealthCheckOptions()
                    {
                        // Exclude all checks and return a 200-Ok.
                        Predicate = (_) => false,
                        ResponseWriter = HealthResponse.WriteResponseAsync
                    }).RequireCors(CorsPolicyNames.AllowAny);
                    endpoints.MapGet("/nodeid", async (context) =>
                    {
                        await context.Response.WriteAsync(Node.Id, Encoding.UTF8);
                    }).RequireCors(CorsPolicyNames.AllowAny);
                })

                .UseSwagger()
                .UseCustomSwaggerUI(oauthOptions);
        }
    }
}
