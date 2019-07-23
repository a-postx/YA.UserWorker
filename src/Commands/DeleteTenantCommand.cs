using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Constants;
using YA.TenantWorker.DAL;
using YA.TenantWorker.MessageBus;
using YA.TenantWorker.Models;

namespace YA.TenantWorker.Commands
{
    public class DeleteTenantCommand : IDeleteTenantCommand
    {
        public DeleteTenantCommand(ILogger<DeleteTenantCommand> logger,
            IActionContextAccessor actionContextAccessor,
            ITenantManagerDbContext managerDbContext,
            IMessageBusServices messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _managerDbContext = managerDbContext ?? throw new ArgumentNullException(nameof(managerDbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<DeleteTenantCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantManagerDbContext _managerDbContext;
        private readonly IMessageBusServices _messageBus;

        public async Task<IActionResult> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            if (tenantId == Guid.Empty)
            {
                return new NotFoundResult();
            }

            Guid correlationId = _actionContextAccessor.GetCorrelationIdFromContext();

            using (_log.BeginScopeWith((Logs.TenantId, tenantId), (Logs.CorrelationId, correlationId)))
            {
                Tenant tenant = await _managerDbContext.GetTenantAsync(tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new NotFoundResult();
                }

                try
                {
                    _managerDbContext.DeleteTenant(tenant);
                    await _managerDbContext.ApplyChangesAsync(cancellationToken);

                    await _messageBus.DeleteTenantV1(tenant.TenantID, correlationId, cancellationToken);
                }
                catch (Exception e) when (_log.LogException(e))
                {
                    throw;
                }

                return new NoContentResult();
            }
        }
    }
}
