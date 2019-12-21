using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;

namespace YA.TenantWorker
{
    enum OsPlatforms
    {
        Unknown = 0,
        Windows = 1,
        Linux = 2,
        OSX = 4
    }

    public class Program
    {
        internal static readonly string AppName = Assembly.GetEntryAssembly()?.GetName().Name;
        internal static readonly Version Version = Assembly.GetEntryAssembly()?.GetName().Version;
        internal static readonly string RootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        internal static Countries Country { get; private set; }        
        internal static OsPlatforms OsPlatform { get; private set; }
        internal static string DotNetVersion { get; private set; }

        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            GetEnvironmentInfo();

            Directory.CreateDirectory(Path.Combine(RootPath, General.AppDataFolderName));


            IHostBuilder builder = CreateHostBuilder(args);

            IHost host;

            try
            {
                Console.WriteLine("Building Host...");

                host = builder.Build();

                Console.WriteLine("Host built successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building Host: {e}.");
                return 1;
            }

            try
            {
                Log.Logger = CreateLogger(host);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building logger: {e}.");
                return 1;
            }

            Log.Information("{AppName} v{Version}", AppName, Version);

            IGeoDataService geoService = host.Services.GetService<IGeoDataService>();
            Country = await geoService.GetCountryCodeAsync();

            try
            {
                await host.RunAsync();
                Log.Information("{AppName} v{Version} has stopped.", AppName, Version);
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "{AppName} terminated unexpectedly.", AppName);
                return 1;
            }
            finally
            {
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
                .UseSerilog()
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
                .UseShutdownTimeout(TimeSpan.FromSeconds(General.SystemShutdownTimeoutSec))
                .UseStartup<Startup>();
        }

        private static IConfigurationBuilder AddConfiguration(IConfigurationBuilder configurationBuilder, IHostEnvironment hostingEnvironment, string[] args)
        {
            configurationBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                // Add configuration specific to Development, Staging or Production environments
                // (launchSettings.json for development or Application Settings for Azure)
                .AddEnvironmentVariables()

                .AddApplicationInsightsSettings(developerMode: !hostingEnvironment.IsProduction())

                // Add command line options. These take the highest priority.
                .AddIf(
                    args != null,
                    x => x.AddCommandLine(args));

            // <##Azure Key Vault
            // disable telemetry to speed up the process - https://docs.microsoft.com/en-us/cli/azure/azure-cli-configuration?view=azure-cli-latest#cli-configuration-values-and-environment-variables
            string keyVaultEndpoint = null;
            string clientId = Environment.GetEnvironmentVariable("KeyVaultAppClientId");
            string clientSecret = Environment.GetEnvironmentVariable("KeyVaultAppClientSecret");

            if (hostingEnvironment.IsDevelopment())
            {
                Console.WriteLine("Hosting environment is Development");
                keyVaultEndpoint = General.DevelopmentKeyVault;

                IgnoreInvalidCertificates();
            }
            else if (hostingEnvironment.IsProduction())
            {
                Console.WriteLine("Hosting environment is Production");
                keyVaultEndpoint = General.ProductionKeyVault;
            }

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                KeyVaultClient keyVaultClient;

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret)) // connect via App Registration
                {
                    keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
                    {
                        ClientCredential adCredential = new ClientCredential(clientId, clientSecret);
                        AuthenticationContext authenticationContext = new AuthenticationContext(authority, null);
                        AuthenticationResult authResult = await authenticationContext.AcquireTokenAsync(resource, adCredential);

                        if (authResult == null)
                        {
                            throw new Exception("Failed to obtain Azure Key Vault JWT token");
                        }

                        return authResult.AccessToken;
                    });
                }
                else // connect via Azure Token Provider
                {
                    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                    keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                }

                configurationBuilder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
            }
            // Azure Key Vault##>

            return configurationBuilder;
        }

        private static Logger CreateLogger(IHost host)
        {
            IHostEnvironment hostEnv = host.Services.GetRequiredService<IHostEnvironment>();
            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();

            KeyVaultSecrets secrets = configuration.Get<KeyVaultSecrets>();

            LoggerConfiguration loggerConfig = new LoggerConfiguration();

            loggerConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("AppName", AppName)
                .Enrich.WithProperty("Version", Version.ToString())
                .Enrich.WithProperty("NodeId", Node.Id)
                .Enrich.WithProperty("ProcessId", Process.GetCurrentProcess().Id)
                .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("EnvironmentName", hostEnv.EnvironmentName)
                .Enrich.WithProperty("EnvironmentUserName", Environment.UserName)
                .Enrich.WithProperty("OSPlatform", OsPlatform.ToString())
                .Enrich.FromMassTransitMessage()
                .Enrich.FromCustomMbEvent()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    //speed up EF Core exception destructuring
                    .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));

            if (!string.IsNullOrEmpty(secrets.AppInsightsInstrumentationKey))
            {
                loggerConfig.WriteTo.ApplicationInsights(secrets.AppInsightsInstrumentationKey, new TraceTelemetryConverter(), LogEventLevel.Debug);
            }

            if (!string.IsNullOrEmpty(secrets.LogzioToken))
            {
                loggerConfig.WriteTo.Logzio(secrets.LogzioToken, 10, TimeSpan.FromSeconds(10), null, LogEventLevel.Debug);
            }

            Logger logger = loggerConfig.CreateLogger();

            if (string.IsNullOrEmpty(secrets.AppInsightsInstrumentationKey) && string.IsNullOrEmpty(secrets.LogzioToken))
            {
                logger.Warning("Sending logs to remote log managment systems is disabled.");
            }
            
            return logger;
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

        private static void GetEnvironmentInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OsPlatform = OsPlatforms.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OsPlatform = OsPlatforms.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OsPlatform = OsPlatforms.OSX;
            }

            DotNetVersion = GetNetCoreVersion();
        }

        // See: https://github.com/dotnet/BenchmarkDotNet/issues/448#issuecomment-308424100
        private static string GetNetCoreVersion()
        {
            Assembly assembly = typeof(GCSettings).Assembly;
            string[] assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            return netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2
                ? assemblyPath[netCoreAppIndex + 1]
                : null;
        }

        private static void IgnoreInvalidCertificates()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) =>
            {
                switch (sslPolicyErrors)
                {
                    case System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors:
                    case System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch:
                    case System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable:
                        break;
                }
                return true;
            };
        }
    }
}