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
    public class GetTenantByIdCommand : IRequest<ICommandResult<TenantVm>>
    {
        public GetTenantByIdCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; protected set; }

        public class GetTenantByIdHandler : IRequestHandler<GetTenantByIdCommand, ICommandResult<TenantVm>>
        {
            public GetTenantByIdHandler(ILogger<GetTenantByIdHandler> logger,
                ITenantWorkerDbContext dbContext,
                IMapper<Tenant, TenantVm> tenantVmMapper)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            }

            private readonly ILogger<GetTenantByIdHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

            public async Task<ICommandResult<TenantVm>> Handle(GetTenantByIdCommand command, CancellationToken cancellationToken)
            {
                Guid tenantId = command.Id;

                if (tenantId == Guid.Empty)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.BadRequest, null);
                }

                Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

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
