using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class DeleteTenantCommand : IDeleteTenantCommand
    {
        public DeleteTenantCommand(ILogger<DeleteTenantCommand> logger,
            IMapper mapper,
            ITenantWorkerDbContext dbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<DeleteTenantCommand> _log;
        private readonly IMapper _mapper;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Tenant tenant = await _dbContext.GetTenantAsync(cancellationToken);

            if (tenant == null)
            {
                return new NotFoundResult();
            }

            _dbContext.DeleteTenant(tenant);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.TenantDeletedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

            return new NoContentResult();
        }
    }
}
