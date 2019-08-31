using System;
using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;

namespace YA.TenantWorker.Messaging
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
            _log.LogInformation("{ContextType}{DestinationAddress}{SourceAddress}{CorrelationId}{Message}",
                metadata.ContextType, metadata.DestinationAddress, metadata.SourceAddress, metadata.CorrelationId, message.ToJson());
            return Task.CompletedTask;
        }
    }
}
