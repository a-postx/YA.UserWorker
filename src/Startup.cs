using System.Reflection;
using System.Text;
using CorrelationId;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using YA.Common.Constants;
using YA.UserWorker.Infrastructure.Health;
using YA.UserWorker.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Delobytes.AspNetCore.Logging;
using Delobytes.AspNetCore.Idempotency;
using Delobytes.AspNetCore;
using YA.UserWorker.Extensions;
using YA.UserWorker.Infrastructure.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
//using Elastic.Apm.NetCoreAll;

namespace YA.UserWorker;

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


    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddCustomOptions(_config)
            .AddOptionsAndSecretsValidationOnStartup();

        AppSecrets secrets = _config.GetSection(nameof(AppSecrets)).Get<AppSecrets>();
        GeneralOptions generalOptions = _config.GetSection(nameof(ApplicationOptions.General)).Get<GeneralOptions>();
        OauthOptions oauthOptions = _config.GetSection(nameof(ApplicationOptions.OAuth)).Get<OauthOptions>();
        IdempotencyOptions idempotencyOptions = _config
            .GetSection(nameof(ApplicationOptions.IdempotencyControl)).Get<IdempotencyOptions>();

        services
            .AddCorrelationIdFluent(generalOptions)
            .AddCustomCaching(secrets)
            .AddCustomCors()
            .AddCustomRouting()
            .AddResponseCaching()
            ////.AddCustomResponseCompression(_config)
            .AddCustomHealthChecks(secrets)
            .AddCustomSwagger()
            .AddFluentValidationRulesToSwagger()
            .AddHttpContextAccessor()
            .AddCustomApiVersioning();

        //services.AddAuth0Authentication("YaScheme", true, options =>
        //{
        //    options.Authority = oauthOptions.Authority;
        //    options.Audience = oauthOptions.Audience;
        //    options.LoginRedirectPath = "/authentication/login";
        //    options.ApiGatewayHost = oauthOptions.ApiGatewayHost;
        //    options.ApiGatewayPort = oauthOptions.ApiGatewayPort;
        //    options.AppMetadataClaimName = "http://myapp.app_metadata";
        //    options.CustomClaims = new List<string> { "http://myapp.email", "http://myapp.email_verified" };
        //    options.OpenIdConfigurationEndpoint = oauthOptions.OidcIssuer + ".well-known/openid-configuration";
        //    options.TokenValidationParameters = new TokenValidationOptions
        //    {
        //        RequireExpirationTime = true,
        //        RequireSignedTokens = true,
        //        ValidateIssuer = true,
        //        ValidIssuer = oauthOptions.Authority + "/",
        //        ValidateAudience = true,
        //        ValidAudience = oauthOptions.Audience,
        //        ValidateIssuerSigningKey = true,
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.FromMinutes(2),
        //    };
        //});

        services.AddKeyCloakAuthentication("YaScheme", true, options =>
        {
            options.Authority = oauthOptions.Authority;
            options.Audience = oauthOptions.Audience;
            options.OpenIdConfigurationEndpoint = oauthOptions.OidcIssuer + "/.well-known/openid-configuration";
            options.LoginRedirectPath = "/authentication/login";
            options.ApiGatewayHost = oauthOptions.ApiGatewayHost;
            options.ApiGatewayPort = oauthOptions.ApiGatewayPort;
            options.CustomClaims = new List<string> { "email", "email_verified", "tid", "tenantaccesstype" };
            options.TokenValidationParameters = new TokenValidationOptions
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = oauthOptions.Authority,
                ValidateAudience = true,
                ValidAudience = oauthOptions.Audience,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };
        });

        services.AddClaimsLogging();

        services.AddIdempotencyContextLogging(options =>
        {
            options.IdempotencyLogAttribute = "IdempotencyKey";
        });

        services.AddHttpContextLogging(options =>
        {
            options.LogRequestBody = true;
            options.LogResponseBody = true;
            options.MaxBodyLength = generalOptions.MaxLogFieldLength;
            options.SkipPaths = new List<PathString> { "/metrics" };
        });

        services
            .AddControllers()
                .AddCustomJsonOptions(_webHostEnvironment)
                ////.AddXmlDataContractSerializerFormatters()
                .AddCustomMvcOptions(_config)
                .AddCustomApiBehavior();

        services.AddCustomModelValidation();

        //есть зависимость от настроек MVC и IDistributedCache
        services.AddIdempotencyControl(options =>
        {
            options.Enabled = idempotencyOptions.IdempotencyFilterEnabled ?? false;
            options.HeaderRequired = true;
            options.IdempotencyHeader = idempotencyOptions.IdempotencyHeader;
        });

        services.AddCustomAuthorization();

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
    }


    public void Configure(IApplicationBuilder application)
    {
        OauthOptions oauthOptions = _config.GetSection(nameof(ApplicationOptions.OAuth)).Get<OauthOptions>();
        IdempotencyControlOptions idempotencyOptions = _config
            .GetSection(nameof(ApplicationOptions.IdempotencyControl)).Get<IdempotencyControlOptions>();

        if (idempotencyOptions.Enabled)
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
            .UseNetworkLogging()

            .UseHttpExceptionHandling(options => options.IncludeStackTraceInResponse = true)

            .UseRouting()
            .UseCors(CorsPolicyNames.AllowAny)
            .UseResponseCaching()
            ////.UseResponseCompression() //перенесена на шлюз
            .UseHttpContextLogging()
            .UseStaticFilesWithCacheControl()
            //временно убираем для сокращения объёма журналов
            ////.UseRouteParamsLogging()

            .UseHealthChecksPrometheusExporter("/metrics")
            .UseMetricServer()
            .UseHttpMetrics()

            .UseAuthentication()
            .UseClaimsLogging()
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
