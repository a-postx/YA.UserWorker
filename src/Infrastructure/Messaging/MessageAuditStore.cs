using System;
using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YA.Common.Constants;
using YA.Common.Extensions;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Messaging
{
    /// <summary>
    /// Хранилище данных аудита сообщений шины данных, использующее простое логирование
    /// </summary>
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

            //оценка целесообразности: корреляционный идентификатор переписывается, если уже существует
            using (_log.BeginScopeWith((YaLogKeys.LogType, LogTypes.MessageBusMessage.ToString()),
                (YaLogKeys.MessageBusContextType, metadata.ContextType),
                (YaLogKeys.MessageBusSourceAddress, metadata.SourceAddress),
                (YaLogKeys.MessageBusDestinationAddress, metadata.DestinationAddress),
                (YaLogKeys.MessageBusMessageId, metadata.MessageId),
                (YaLogKeys.CorrelationId, metadata.CorrelationId),
                (YaLogKeys.MessageBusConversationId, metadata.ConversationId),
                (YaLogKeys.MessageBusMessage, savedMessage)))
            {
                _log.LogInformation("{MessageBusContextType} message bus message {MessageBusMessageId}", metadata.ContextType, metadata.MessageId);
            }

            return Task.CompletedTask;
        }
    }
}
