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
    public class GetTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public class GetTenantHandler : IRequestHandler<GetTenantCommand, ICommandResult<Tenant>>
        {
            public GetTenantHandler(ILogger<GetTenantHandler> logger,
                ITenantWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<GetTenantHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;

            public async Task<ICommandResult<Tenant>> Handle(GetTenantCommand command, CancellationToken cancellationToken)
            {
                Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<Tenant>(CommandStatus.NotFound, null);
                }

                return new CommandResult<Tenant>(CommandStatus.Ok, tenant);
            }
        }
    }
}
