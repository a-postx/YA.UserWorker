global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;

using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prometheus.DotNetRuntime;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Extensions.Hosting;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using YA.UserWorker.Infrastructure.Data;
using YA.UserWorker.Options;
using YA.UserWorker.Constants;
using YA.UserWorker.Extensions;
using YA.UserWorker.Application.Interfaces;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Globalization;

[assembly: CLSCompliant(false)]
namespace YA.UserWorker;

internal enum OsPlatforms
{
    Unknown = 0,
    Windows = 1,
    Linux = 2,
    OSX = 4
}

public static class Program
{
    internal static readonly string AppName = Assembly.GetEntryAssembly()?.GetName().Name;
    internal static readonly Version AppVersion = Assembly.GetEntryAssembly()?.GetName().Version;
    internal static readonly string RootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

    internal static RuntimeCountry Country { get; private set; }        
    internal static OsPlatforms OsPlatform { get; private set; }

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        Log.Logger = CreateBootstrapLogger();

        IDisposable dotNetRuntimeStats = null;
        IHostEnvironment hostEnvironment = null;

        try
        {
            Log.Information("Building Host...");

            OsPlatform = GetOs();

            IHost host = CreateHostBuilder(args).Build();

            Log.Information("Host built successfully.");

            hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
            Log.Information("Hosting environment is {EnvironmentName}", hostEnvironment.EnvironmentName);

            //!!! автомиграция основной БД - норм для разработки, но на проде делать через скрипты !!!
            try
            {
                using (IServiceScope scope = host.Services.CreateScope())
                {
                    UserWorkerDbContext dbContext = scope.ServiceProvider.GetService<UserWorkerDbContext>();

                    using (CancellationTokenSource cts = new CancellationTokenSource(120000))
                    {
                        if (dbContext.Database.GetPendingMigrations().GetEnumerator().MoveNext())
                        {
                            Log.Information("Applying database migrations...");

                            await dbContext.Database.MigrateAsync(cts.Token);

                            Log.Information("Database migrations applied successfully.");
                        }
                        else
                        {
                            Log.Information("No database migrations needed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying database migration");
                return 1;
            }

            string coreCLR = ((AssemblyInformationalVersionAttribute[])typeof(object).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0].InformationalVersion;
            string coreFX = ((AssemblyInformationalVersionAttribute[])typeof(Uri).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0].InformationalVersion;

            Log.Information("Application.Name: {AppName}\n Application.Version: {AppVersion}\n " +
                "Environment.Version: {EnvVersion}\n RuntimeInformation.FrameworkDescription: {RuntimeInfo}\n " +
                "CoreCLR Build: {CoreClrBuild}\n CoreCLR Hash: {CoreClrHash}\n " +
                "CoreFX Build: {CoreFxBuild}\n CoreFX Hash: {CoreFxHash}\n " +
                "Environment.OSVersion {OsVersion}\n RuntimeInformation.OSDescription: {OsDescr}\n " +
                "RuntimeInformation.OSArchitecture: {OsArch}\n Environment.ProcessorCount: {CpuCount}",
                AppName, AppVersion, Environment.Version, RuntimeInformation.FrameworkDescription, coreCLR.Split('+')[0],
                coreCLR.Split('+')[1], coreFX.Split('+')[0], coreFX.Split('+')[1], Environment.OSVersion,
                RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture, Environment.ProcessorCount);

            ThreadPool.SetMinThreads(100, 100); //согласно лучшим практикам азуровского редиса
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
            Log.Information("Min worker threads: {WorkerThreads}, min IO threads: {IoThreads}", minWorkerThreads, minIOThreads);

            IRuntimeGeoDataService geoService = host.Services.GetService<IRuntimeGeoDataService>();
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                Country = await geoService.GetCountryCodeAsync(cts.Token);
            }

            IHostApplicationLifetime hostLifetime = host.Services.GetService<IHostApplicationLifetime>();
            hostLifetime.ApplicationStopping.Register(() =>
            {
                host.Services.GetRequiredService<ILogger<Startup>>().LogInformation("Shutdown has been initiated.");
            });

            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
            //вызывается дважды при изменениях на файловой системе https://github.com/dotnet/aspnetcore/issues/2542
            ChangeToken.OnChange(configuration.GetReloadToken, () =>
            {
                host.Services.GetRequiredService<ILogger<Startup>>().LogInformation("Options or secrets has been modified.");
            });

            dotNetRuntimeStats = DotNetRuntimeStatsBuilder.Default().StartCollecting();

            await host.RunAsync().ConfigureAwait(false);

            Log.Information("{AppName} has stopped.", AppName);
            return 0;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            Log.Fatal(ex, "{AppName} terminated unexpectedly in {Environment} mode.", AppName, hostEnvironment?.EnvironmentName);
            return 1;
        }
        finally
        {
            dotNetRuntimeStats?.Dispose();
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder hostBuilder = new HostBuilder().UseContentRoot(Directory.GetCurrentDirectory());
        hostBuilder
            .ConfigureHostConfiguration(
                configurationBuilder => configurationBuilder
                    .AddEnvironmentVariables(prefix: "DOTNET_")
                    .AddIf(
                        args != null,
                        x => x.AddCommandLine(args)))
            .ConfigureAppConfiguration((hostingContext, config) =>
                AddConfiguration(config, hostingContext.HostingEnvironment, args))
            .UseSerilog(ConfigureReloadableLogger)
            .UseDefaultServiceProvider(
                (context, options) =>
                {
                    bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                })
            .ConfigureWebHost(ConfigureWebHostBuilder);

        return hostBuilder;
    }

    private static void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder
            .UseKestrel(
                (builderContext, options) =>
                {
                    options.AddServerHeader = false;

                    options.Configure(builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)));
                    ConfigureKestrelServerLimits(builderContext, options);
                })
            //<##AzureAppServicesIntegration
            .UseAzureAppServices()
            .UseSetting("detailedErrors", "true")
            .CaptureStartupErrors(true)
            //AzureAppServicesIntegration##>

            // Used for IIS and IIS Express for in-process hosting. Use UseIISIntegration for out-of-process hosting.
            .UseIIS()
            .UseShutdownTimeout(TimeSpan.FromSeconds(Timeouts.WebHostShutdownTimeoutSec))
            .UseStartup<Startup>();
    }

    private static IConfigurationBuilder AddConfiguration(IConfigurationBuilder configurationBuilder, IHostEnvironment hostingEnvironment, string[] args)
    {
        IConfigurationRoot tempConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: false)
            .Build();

        configurationBuilder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        string envParameterStorePath = hostingEnvironment.EnvironmentName.ToLowerInvariant();

        configurationBuilder.AddYandexCloudLockboxConfiguration(config =>
        {
            config.PrivateKey = Environment.GetEnvironmentVariable("YC_PRIVATE_KEY");
            config.ServiceAccountId = tempConfig.GetValue<string>("YC:ServiceAccountId");
            config.ServiceAccountAuthorizedKeyId = tempConfig.GetValue<string>("YC:ServiceAccountAuthorizedKeyId");
            config.SecretId = tempConfig.GetValue<string>("YC:ConfigurationSecretId");
            config.PathSeparator = '-';
            config.Optional = false;
            config.ReloadPeriod = TimeSpan.FromDays(7);
            config.LoadTimeout = TimeSpan.FromSeconds(20);
            config.OnLoadException += exceptionContext =>
            {
                //log
            };
        });

        // Добавляем параметры командной строки, которые имеют наивысший приоритет.
        configurationBuilder
            .AddIf(
                args != null,
                x => x.AddCommandLine(args));

        return configurationBuilder;
    }

    /// <summary>
    /// Создаёт логер для работы во время инициализации приложения.
    /// </summary>
    /// <returns>Логер, который может загрузить новую конфигурацию.</returns>
    private static ReloadableLogger CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Добавляет расширенный логер с засылкой данных в удалённые системы
    /// </summary>
    private static void ConfigureReloadableLogger(HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfig)
    {
        IHostEnvironment hostEnv = services.GetRequiredService<IHostEnvironment>();
        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        AppSecrets secrets = services.GetRequiredService<IOptions<AppSecrets>>().Value;

        loggerConfig
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("AppName", AppName)
            .Enrich.WithProperty("Version", AppVersion.ToString())
            .Enrich.WithProperty("NodeId", Node.Id)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("EnvironmentName", hostEnv.EnvironmentName)
            .Enrich.WithProperty("EnvironmentUserName", Environment.UserName)
            .Enrich.WithProperty("OSPlatform", OsPlatform.ToString())
            .Enrich.FromMassTransitMessage()
            .Enrich.FromCustomMbMessageContext()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                //speed up EF Core exception destructuring
                .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));

        if (!string.IsNullOrEmpty(secrets.ElasticSearchUrl)
            && !string.IsNullOrEmpty(secrets.ElasticSearchUser)
            && !string.IsNullOrEmpty(secrets.ElasticSearchPassword))
        {
            Dictionary<string, string> customIndexTemplateSettings = new Dictionary<string, string>
            {
                { "index.lifecycle.name", hostEnv.IsProduction() ? "ya-logs-prod-policy" : "ya-logs-dev-policy" },
            };

            loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(secrets.ElasticSearchUrl))
            {
                ModifyConnectionSettings = conn =>
                {
                    conn.BasicAuthentication(secrets.ElasticSearchUser, secrets.ElasticSearchPassword);

                    //"https://rc1b-8k9r4mkxxxxxxxxxx.mdb.yandexcloud.net:9200"
                    ////conn.ServerCertificateValidationCallback(YandexCloudRootCaCertificateValidationCallback);
                    ////conn.EnableDebugMode(conn =>
                    ////{
                    ////    string info = conn.DebugInformation;
                    ////});
                    return conn;
                },
                BatchPostingLimit = 1000,
                Period = TimeSpan.FromSeconds(10),
                MinimumLogEventLevel = LogEventLevel.Information,
                FailureCallback = e => Console.WriteLine("Unable to submit log event to ELK: " + e.MessageTemplate),
                EmitEventFailure = EmitEventFailureHandling.RaiseCallback,
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                OverwriteTemplate = false,
                RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
                TemplateName = hostEnv.IsProduction() ? "ya-logs-prod-template" : "ya-logs-dev-template",
                TemplateCustomSettings = customIndexTemplateSettings,
                InlineFields = true,
                IndexFormat = hostEnv.IsProduction() ? "ya-logs-prod-{0:yyyy.MM.dd}" : "ya-logs-dev-{0:yyyy.MM.dd}",
                DeadLetterIndexName = hostEnv.IsProduction() ? "ya-prod-deadletter-{0:yyyy.MM.dd}" : "ya-dev-deadletter-{0:yyyy.MM.dd}",
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true, inlineFields: true),
                BufferCleanPayload = (failingEvent, statuscode, exception) =>
                {
                    dynamic e = JObject.Parse(failingEvent);
                    return JsonConvert.SerializeObject(new Dictionary<string, object>()
                    {
                        { "@timestamp", e["@timestamp"]},
                        { "level", "Error"},
                        { "message", "Error: " + e.message},
                        { "messageTemplate", e.messageTemplate},
                        { "failingStatusCode", statuscode},
                        { "failingException", exception}
                    });
                }
            });
        }
        else
        {
            Log.Warning("Sending logs to remote log managment systems is disabled.");
        }
    }

    private static bool YandexCloudRootCaCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        string rootCAThumbprint = "AAA1450272071C2D8D7F48469886180B7685EF94";

        // remove this line if commercial CAs are not allowed to issue certificate for your service.
        if ((sslPolicyErrors & (SslPolicyErrors.None)) > 0)
        {
            return true;
        }

        if ((sslPolicyErrors & (SslPolicyErrors.RemoteCertificateNameMismatch)) > 0 ||
            (sslPolicyErrors & (SslPolicyErrors.RemoteCertificateNotAvailable)) > 0)
        {
            return false;
        }

        // get last chain element that should contain root CA certificate
        // but this may not be the case in partial chains
        X509Certificate2 projectedRootCert = chain.ChainElements[^1].Certificate;

        return projectedRootCert.Thumbprint == rootCAThumbprint;
    }

    /// <summary>
    /// Configure Kestrel server limits from appsettings.json is not supported so we manually copy from config.
    /// https://github.com/aspnet/KestrelHttpServer/issues/2216
    /// </summary>
    private static void ConfigureKestrelServerLimits(WebHostBuilderContext builderContext, KestrelServerOptions options)
    {
        KestrelServerOptions source = new KestrelServerOptions();
        builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)).Bind(source);

        KestrelServerLimits limits = options.Limits;
        KestrelServerLimits sourceLimits = source.Limits;

        Http2Limits http2 = limits.Http2;
        Http2Limits sourceHttp2 = sourceLimits.Http2;

        http2.HeaderTableSize = sourceHttp2.HeaderTableSize;
        http2.InitialConnectionWindowSize = sourceHttp2.InitialConnectionWindowSize;
        http2.InitialStreamWindowSize = sourceHttp2.InitialStreamWindowSize;
        http2.MaxFrameSize = sourceHttp2.MaxFrameSize;
        http2.MaxRequestHeaderFieldSize = sourceHttp2.MaxRequestHeaderFieldSize;
        http2.MaxStreamsPerConnection = sourceHttp2.MaxStreamsPerConnection;

        limits.KeepAliveTimeout = sourceLimits.KeepAliveTimeout;
        limits.MaxConcurrentConnections = sourceLimits.MaxConcurrentConnections;
        limits.MaxConcurrentUpgradedConnections = sourceLimits.MaxConcurrentUpgradedConnections;
        limits.MaxRequestBodySize = sourceLimits.MaxRequestBodySize;
        limits.MaxRequestBufferSize = sourceLimits.MaxRequestBufferSize;
        //Azure App Service add > 20 headers
        limits.MaxRequestHeaderCount = sourceLimits.MaxRequestHeaderCount;
        limits.MaxRequestHeadersTotalSize = sourceLimits.MaxRequestHeadersTotalSize;
        //https://github.com/aspnet/AspNetCore/issues/12614
        limits.MaxRequestLineSize = sourceLimits.MaxRequestLineSize - 10;
        limits.MaxResponseBufferSize = sourceLimits.MaxResponseBufferSize;
        limits.MinRequestBodyDataRate = sourceLimits.MinRequestBodyDataRate;
        limits.MinResponseDataRate = sourceLimits.MinResponseDataRate;
        limits.RequestHeadersTimeout = sourceLimits.RequestHeadersTimeout;
    }

    private static OsPlatforms GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OsPlatforms.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OsPlatforms.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OsPlatforms.OSX;
        }
        else
        {
            return OsPlatforms.Unknown;
        }
    }
}
