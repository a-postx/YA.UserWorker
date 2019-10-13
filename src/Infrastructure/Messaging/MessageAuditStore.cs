using System;
using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Messaging
{
    public class MessageAuditStore : IMessageAuditStore
    {
        public MessageAuditStore(ILogger<MessageAuditStore> logger)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly ILogger<MessageAuditStore> _log;

        public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
        {
            string savedMessage = JToken.Parse(JsonConvert.SerializeObject(message)).ToString(Formatting.Indented);

            //logz.io/logstash fields can accept only 32k strings so request/response bodies are cut
            if (savedMessage.Length > General.MaxLogFieldLength)
            {
                savedMessage = savedMessage.Substring(0, General.MaxLogFieldLength);
            }

            _log.LogInformation("{ContextType}{DestinationAddress}{SourceAddress}{CorrelationId}{Message}",
                metadata.ContextType, metadata.DestinationAddress, metadata.SourceAddress, metadata.CorrelationId, savedMessage);
            return Task.CompletedTask;
        }
    }
}
