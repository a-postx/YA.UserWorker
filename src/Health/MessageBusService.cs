using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Health
{
    /// <summary>
    /// Startup check for availability of a Message Bus
    /// </summary>
    public class MessageBusService : IHostedService, IDisposable
    {
        private readonly ILogger<MessageBusService> _log;
        private readonly IConfiguration _config;
        private readonly MessageBusServiceHealthCheck _messageBusServiceHealthCheck;

        public MessageBusService(ILogger<MessageBusService> logger, IConfiguration config, MessageBusServiceHealthCheck messageBusServiceHealthCheck)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _messageBusServiceHealthCheck = messageBusServiceHealthCheck ?? throw new ArgumentNullException(nameof(messageBusServiceHealthCheck));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " startup service is starting.");

            KeyVaultSecrets secrets = _config.Get<KeyVaultSecrets>();

            bool success = false;

            while(!success)
            {
                bool result = false;

                try
                {
                    result = await Utils.CheckTcpConnectionAsync(secrets.MessageBusHost, General.MessageBusServiceHealthPort);
                }
                catch (Exception e)
                {
                    _log.LogError(nameof(MessageBusService) + " startup service has failed: {Exception}", e);
                }

                if (result)
                {
                    success= true;
                    _messageBusServiceHealthCheck.MessageBusStartupTaskCompleted = true;

                    _log.LogInformation(nameof(MessageBusService) + " startup service has started.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation(nameof(MessageBusService) + " startup service is stopping.");

            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
