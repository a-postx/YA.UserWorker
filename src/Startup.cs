using System;
using System.Reflection;
using System.Text;
using Amazon.Extensions.NETCore.Setup;
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
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Middlewares.ResourceFilters;
using YA.UserWorker.Extensions;
using YA.UserWorker.Infrastructure.Health;
using YA.UserWorker.Infrastructure.Authentication;
using YA.UserWorker.Infrastructure.Caching;
using YA.UserWorker.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
//using Elastic.Apm.NetCoreAll;

namespace YA.UserWorker
{
    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

            Configuration = configuration;
        }

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // controller design generator search for this
        private IConfiguration Configuration { get; }

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
            IdempotencyControlOptions idempotencyOptions = _config.GetSection(nameof(ApplicationOptions.IdempotencyControl)).Get<IdempotencyControlOptions>();

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

                .AddCustomCaching(secrets)
                .AddCustomCors()
                .AddCustomRouting()
                .AddResponseCaching()
                .AddCustomResponseCompression(_config)
                .AddCustomHealthChecks(secrets)
                .AddCustomSwagger()
                .AddFluentValidationRulesToSwagger()
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

            services.AddHttpClient();
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services
                .AddProjectActionHandlers()
                .AddProjectComponents()
                .AddProjectMappers()
                .AddProjectRepositories()
                .AddProjectServices();

            services.AddCustomDatabase(secrets, _webHostEnvironment);

            services.AddCustomMessageBus(secrets);

            services.AddScoped<IdempotencyFilterAttribute>();
            services.AddScoped<IApiRequestDistributedCache, ApiRequestRedisCache>();
            services.AddSingleton<IApiRequestMemoryCache, ApiRequestMemoryCache>();
        }

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after IHost Run() by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application)
        {
            OauthOptions oauthOptions = _config.GetSection(nameof(ApplicationOptions.OAuth)).Get<OauthOptions>();
            IdempotencyControlOptions idempotencyOptions = _config.GetSection(nameof(ApplicationOptions.IdempotencyControl)).Get<IdempotencyControlOptions>();

            if (idempotencyOptions.IdempotencyFilterEnabled.HasValue && idempotencyOptions.IdempotencyFilterEnabled.Value)
            {
                application.UseIdempotencyContextLogging();
            }

            application
                .UseCorrelationId()

                //.UseAllElasticApm(Configuration)

                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
                })
                .UseNetworkContextLogging()

                .UseCustomExceptionHandler()

                .UseRouting()
                .UseCors(CorsPolicyNames.AllowAny)
                .UseResponseCaching()
                .UseResponseCompression()
                .UseHttpContextLogging()
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
                    endpoints.MapHealthChecks("/elkmetrics", new HealthCheckOptions()
                    {
                        Predicate = (check) => check.Tags.Contains("metric"),
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
