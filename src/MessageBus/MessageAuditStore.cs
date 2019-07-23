﻿using System;
using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;

namespace YA.TenantWorker.MessageBus
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
            _log.LogInformation("{ContextType}{CorrelationId}{Message}", metadata.ContextType, metadata.CorrelationId, message.ToJson());
            return Task.CompletedTask;
        }
    }
}
