using Delobytes.Mapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.CommandsAndQueries.Tenants.Queries
{
    public class GetTenantCommand : IRequest<ICommandResult<TenantVm>>
    {
        public class GetTenantHandler : IRequestHandler<GetTenantCommand, ICommandResult<TenantVm>>
        {
            public GetTenantHandler(ILogger<GetTenantHandler> logger,
                ITenantWorkerDbContext dbContext,
                IMapper<Tenant, TenantVm> tenantVmMapper)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            }

            private readonly ILogger<GetTenantHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

            public async Task<ICommandResult<TenantVm>> Handle(GetTenantCommand command, CancellationToken cancellationToken)
            {
                Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.NotFound, null);
                }

                TenantVm tenantViewModel = _tenantVmMapper.Map(tenant);

                return new CommandResult<TenantVm>(CommandStatuses.Ok, tenantViewModel);
            }
        }
    }
}
