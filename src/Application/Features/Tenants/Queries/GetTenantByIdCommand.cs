using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.Tenants.Queries
{
    public class GetTenantByIdCommand : IRequest<ICommandResult<Tenant>>
    {
        public GetTenantByIdCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; protected set; }

        public class GetTenantByIdHandler : IRequestHandler<GetTenantByIdCommand, ICommandResult<Tenant>>
        {
            public GetTenantByIdHandler(ILogger<GetTenantByIdHandler> logger,
                ITenantWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<GetTenantByIdHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;

            public async Task<ICommandResult<Tenant>> Handle(GetTenantByIdCommand command, CancellationToken cancellationToken)
            {
                Guid tenantId = command.Id;

                if (tenantId == Guid.Empty)
                {
                    return new CommandResult<Tenant>(CommandStatus.BadRequest, null);
                }

                Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<Tenant>(CommandStatus.NotFound, null);
                }

                return new CommandResult<Tenant>(CommandStatus.Ok, tenant);
            }
        }
    }
}
