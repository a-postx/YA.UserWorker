using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class DeleteTenantCommand : IDeleteTenantCommand
    {
        public DeleteTenantCommand(ILogger<DeleteTenantCommand> logger,
            IActionContextAccessor actionContextAccessor,
            ITenantWorkerDbContext workerDbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<DeleteTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<IActionResult> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            Guid correlationId = _actionContextAccessor.GetCorrelationId(General.CorrelationIdHeader);

            if (tenantId == Guid.Empty)
            {
                return new BadRequestResult();
            }

            Tenant tenant = await _dbContext.GetEntityAsync<Tenant>(e => e.TenantID == tenantId, cancellationToken);

            if (tenant == null)
            {
                return new NotFoundResult();
            }

            _dbContext.DeleteTenant(tenant);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.DeleteTenantV1Async(correlationId, tenant.TenantID, new TenantSm { TenantId = tenant.TenantID, TenantName = tenant.TenantName }, cancellationToken);

            return new NoContentResult();
        }
    }
}
