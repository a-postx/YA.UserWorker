using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YA.Common.Constants;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Extensions;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Infrastructure.Messaging
{
    /// <summary>
    /// Хранилище данных аудита сообщений шины данных, использующее простое логирование
    /// </summary>
    public class MessageAuditStore : IMessageAuditStore
    {
        public MessageAuditStore(ILogger<MessageAuditStore> logger, IOptionsMonitor<GeneralOptions> optionsMonitor)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxLogFieldLength = optionsMonitor.CurrentValue.MaxLogFieldLength;
            _serializingOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
        }

        private readonly ILogger<MessageAuditStore> _log;
        private readonly int _maxLogFieldLength;
        private readonly JsonSerializerOptions _serializingOptions;

        public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
        {
            string savedMessage = JsonSerializer.Serialize(message, _serializingOptions);

            // поля logz.io/logstash поддерживают только строки длиной до 32тыс, поэтому обрезаем тела запросов/ответов
            if (savedMessage.Length > _maxLogFieldLength)
            {
                savedMessage = savedMessage.Substring(0, _maxLogFieldLength);
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
