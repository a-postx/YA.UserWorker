using MediatR;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.TenantInvitations.Commands;

public class UpdateInvitationStatusCommand : IRequest<ICommandResult<YaInvitation>>
{
    public UpdateInvitationStatusCommand(Guid id, YaTenantInvitationStatus status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; protected set; }
    public YaTenantInvitationStatus Status { get; protected set; }

    public class UpdateInvitationStatusHandler : IRequestHandler<UpdateInvitationStatusCommand, ICommandResult<YaInvitation>>
    {
        public UpdateInvitationStatusHandler(ILogger<UpdateInvitationStatusHandler> logger,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<UpdateInvitationStatusHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<YaInvitation>> Handle(UpdateInvitationStatusCommand command, CancellationToken cancellationToken)
        {
            Guid invitationId = command.Id;
            YaTenantInvitationStatus status = command.Status;

            if (invitationId == Guid.Empty)
            {
                return new CommandResult<YaInvitation>(CommandStatus.BadRequest, null);
            }

            if (status == YaTenantInvitationStatus.Unknown)
            {
                return new CommandResult<YaInvitation>(CommandStatus.BadRequest, null);
            }

            YaInvitation invitation = await _dbContext
                .GetInvitationAsync(e => e.YaInvitationID == invitationId, cancellationToken);

            if (invitation == null)
            {
                return new CommandResult<YaInvitation>(CommandStatus.NotFound, null);
            }

            invitation.SetStatus(status);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult<YaInvitation>(CommandStatus.Ok, invitation);
        }
    }
}
