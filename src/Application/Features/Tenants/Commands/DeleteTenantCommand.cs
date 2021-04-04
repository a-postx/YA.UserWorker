using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Commands
{
    public class DeleteTenantCommand : IRequest<ICommandResult<EmptyCommandResult>>
    {
        public DeleteTenantCommand(Guid tenantId)
        {
            TenantId = tenantId;
        }

        public Guid TenantId { get; protected set; }

        public class DeleteTenantHandler : IRequestHandler<DeleteTenantCommand, ICommandResult<EmptyCommandResult>>
        {
            public DeleteTenantHandler(ILogger<DeleteTenantHandler> logger,
                IMapper mapper,
                IUserWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<DeleteTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly IUserWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<EmptyCommandResult>> Handle(DeleteTenantCommand command, CancellationToken cancellationToken)
            {
                Guid tenantId = command.TenantId;

                //проверять членства, удалять пользователя, если удаляемый арендатор является последним?

                Tenant tenant = await _dbContext.GetTenantAsync(tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatus.NotFound, null);
                }

                _dbContext.DeleteTenant(tenant);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.TenantDeletedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

                return new CommandResult<EmptyCommandResult>(CommandStatus.Ok, null);
            }
        }
    }
}
