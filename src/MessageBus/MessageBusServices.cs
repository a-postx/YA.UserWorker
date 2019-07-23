using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Delobytes.Mapper;
using MassTransit;
using MbEvents;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;
using YA.TenantWorker.DAL;
using YA.TenantWorker.Models;
using YA.TenantWorker.SaveModels;
using YA.TenantWorker.Services;

namespace YA.TenantWorker.MessageBus
{
    public enum MbOperationStatuses
    {
        Success = 1,
        Error = 2
    }

    public class MessageBusServices : IMessageBusServices
    {
        public MessageBusServices(ILogger<MessageBusServices> logger,
            ITenantManagerDbContext dbContext,
            IBus bus,
            IPublishEndpoint publishEndpoint)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        private readonly ILogger<MessageBusServices> _log;
        private readonly ITenantManagerDbContext _dbContext;
        private readonly IBus _bus;
        private readonly IPublishEndpoint _publishEndpoint;

        public async Task CreateTenantV1(TenantSm tenantSm, Guid correlationId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<ICreateTenantV1>(new { CorrelationId = correlationId, Tenant = tenantSm }, cancellationToken);
        }

        public async Task DeleteTenantV1(Guid tenantId, Guid correlationId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<IDeleteTenantV1>(new { CorrelationId = correlationId, TenantID = tenantId }, cancellationToken);
        }

        public async Task UpdateTenantV1(TenantSm tenantSm, Guid correlationId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish<IUpdateTenantV1>(new { CorrelationId = correlationId, Tenant = tenantSm }, cancellationToken);
        }
    }
}
