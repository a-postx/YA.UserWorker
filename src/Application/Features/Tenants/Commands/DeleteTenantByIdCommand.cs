using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Commands;

public class DeleteTenantByIdCommand : IRequest<ICommandResult>
{
    public DeleteTenantByIdCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }

    public class DeleteTenantByIdHandler : IRequestHandler<DeleteTenantByIdCommand, ICommandResult>
    {
        public DeleteTenantByIdHandler(ILogger<DeleteTenantByIdHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<DeleteTenantByIdHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<ICommandResult> Handle(DeleteTenantByIdCommand command, CancellationToken cancellationToken)
        {
            Guid tenantId = command.Id;

            if (tenantId == Guid.Empty)
            {
                return new CommandResult(CommandStatus.BadRequest);
            }

            Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

            if (tenant == null)
            {
                return new CommandResult(CommandStatus.NotFound);
            }

            _dbContext.DeleteTenant(tenant);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.TenantDeletedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

            return new CommandResult(CommandStatus.Ok);
        }
    }
}
