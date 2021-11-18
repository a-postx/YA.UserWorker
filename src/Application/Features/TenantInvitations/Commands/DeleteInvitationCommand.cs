using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.TenantInvitations.Commands;

public class DeleteInvitationCommand : IRequest<ICommandResult>
{
    public DeleteInvitationCommand(Guid id, Guid tenantId)
    {
        Id = id;
        TenantId = tenantId;
    }

    public Guid Id { get; protected set; }
    public Guid TenantId { get; protected set; }

    public class DeleteInviteHandler : IRequestHandler<DeleteInvitationCommand, ICommandResult>
    {
        public DeleteInviteHandler(ILogger<DeleteInviteHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<DeleteInviteHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<ICommandResult> Handle(DeleteInvitationCommand command, CancellationToken cancellationToken)
        {
            Guid invitationId = command.Id;
            Guid tenantId = command.TenantId;

            if (invitationId == Guid.Empty)
            {
                return new CommandResult(CommandStatus.BadRequest);
            }

            if (tenantId == Guid.Empty)
            {
                return new CommandResult(CommandStatus.BadRequest);
            }

            YaInvitation yaInvite = await _dbContext
                .GetInvitationAsync(e => e.YaInvitationID == invitationId && e.TenantId == tenantId, cancellationToken);

            if (yaInvite == null)
            {
                return new CommandResult(CommandStatus.NotFound);
            }

            _dbContext.DeleteInvitation(yaInvite);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult(CommandStatus.Ok);
        }
    }
}
