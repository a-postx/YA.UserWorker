using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Queries
{
    public class GetTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public GetTenantCommand(Guid tenantId)
        {
            TenantId = tenantId;
        }

        public Guid TenantId { get; protected set; }

        public class GetTenantHandler : IRequestHandler<GetTenantCommand, ICommandResult<Tenant>>
        {
            public GetTenantHandler(ILogger<GetTenantHandler> logger,
                IUserWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<GetTenantHandler> _log;
            private readonly IUserWorkerDbContext _dbContext;

            public async Task<ICommandResult<Tenant>> Handle(GetTenantCommand command, CancellationToken cancellationToken)
            {
                Guid tenantId = command.TenantId;

                Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<Tenant>(CommandStatus.NotFound, null);
                }

                return new CommandResult<Tenant>(CommandStatus.Ok, tenant);
            }
        }
    }
}
