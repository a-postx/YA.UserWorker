using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;
using YA.TenantWorker.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;

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
        internal static readonly string AppName = Assembly.GetEntryAssembly().GetName().Name;
        internal static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;
        internal static readonly string RootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        internal static readonly int ProcessId = Process.GetCurrentProcess().Id;
        internal static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;
        internal static readonly string MachineName = Environment.MachineName;
        internal static readonly string UserName = Environment.UserName;

        internal static string NodeId { get; private set; }
        internal static Countries Country { get; private set; }
        
        internal static OsPlatforms OsPlatform { get; private set; }

        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            GetEnvironmentInfo();

            Directory.CreateDirectory(Path.Combine(RootPath, General.AppDataFolderName));
            NodeId = Node.Id;

            IWebHostBuilder builder = CreateWebHostBuilder(args);

            IWebHost webHost;

            try
            {
                Console.WriteLine("Building WebHost...");

                webHost = builder.Build();

                Console.WriteLine("WebHost built successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building WebHost: {e.Message}.\nPlease check Internet connection.");
                return 1;
            }

            try
            {
                Log.Logger = BuildLogger(webHost);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building logger: {e.Message}.\nRemote logs is disabled.");
            }

            Log.Information("{AppName} v{Version}", AppName, Version);

            IGeoDataService geoService = webHost.Services.GetService<IGeoDataService>();
            Country = await geoService.GetCountryCodeAsync();

            try
            {
                await webHost.RunAsync();
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseIf(
                    x => string.IsNullOrEmpty(x.GetSetting(WebHostDefaults.ContentRootKey)),
                    x => x.UseContentRoot(Directory.GetCurrentDirectory()))
                .UseIf(
                    args != null,
                    x => x.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build()))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    AddConfiguration(config, hostingContext.HostingEnvironment, args);

                    // <##Azure Key Vault
                    string keyVaultEndpoint = null;

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        keyVaultEndpoint = General.DevelopmentKeyVault;
                    }
                    else if (hostingContext.HostingEnvironment.IsProduction())
                    {
                        keyVaultEndpoint = General.ProductionKeyVault;
                    }

                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                        KeyVaultClient keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                        config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                    }
                    // Azure Key Vault##>
                })
                .UseSerilog()
                .UseDefaultServiceProvider((context, options) =>
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
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

        private static IConfigurationBuilder AddConfiguration(IConfigurationBuilder configurationBuilder, IHostingEnvironment hostingEnvironment, string[] args)
        {
            configurationBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true,
                    reloadOnChange: true)
                // Add configuration specific to Development, Staging or Production environments
                // (launchSettings.json for development or Application Settings for Azure)
                .AddEnvironmentVariables()

                // View telemetry results immediately in development and staging environments.
                .AddApplicationInsightsSettings(developerMode: !hostingEnvironment.IsProduction())

                // Add command line options. These take the highest priority.
                .AddIf(
                    args != null,
                    x => x.AddCommandLine(args));

            return configurationBuilder;
        }

        private static Logger BuildLogger(IWebHost webHost)
        {
            IConfiguration configuration = webHost.Services.GetRequiredService<IConfiguration>();
            KeyVaultSecrets secrets = configuration.Get<KeyVaultSecrets>();

            LoggerConfiguration loggerConfig = new LoggerConfiguration();

            loggerConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("AppName", AppName)
                .Enrich.WithProperty("Version", Version.ToString())
                .Enrich.WithProperty("NodeId", NodeId)
                .Enrich.WithProperty("ProcessId", ProcessId)
                .Enrich.WithProperty("ProcessName", ProcessName)
                .Enrich.WithProperty("MachineName", MachineName)
                .Enrich.WithProperty("EnvironmentUserName", UserName)
                .Enrich.WithProperty("OSPlatform", OsPlatform.ToString());

            if (!string.IsNullOrEmpty(secrets.AppInsightsInstrumentationKey))
            {
                loggerConfig.WriteTo.ApplicationInsightsTraces(secrets.AppInsightsInstrumentationKey, LogEventLevel.Debug);
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
            limits.MaxRequestHeaderCount = sourceLimits.MaxRequestHeaderCount;
            limits.MaxRequestHeadersTotalSize = sourceLimits.MaxRequestHeadersTotalSize;
            limits.MaxRequestLineSize = sourceLimits.MaxRequestLineSize;
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
        }
    }
}