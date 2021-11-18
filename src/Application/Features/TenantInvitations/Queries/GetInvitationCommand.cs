using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.TenantInvitations.Queries;

public class GetInvitationCommand : IRequest<ICommandResult<YaInvitation>>
{
    public GetInvitationCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }

    public class GetInviteHandler : IRequestHandler<GetInvitationCommand, ICommandResult<YaInvitation>>
    {
        public GetInviteHandler(ILogger<GetInviteHandler> logger,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<GetInviteHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<YaInvitation>> Handle(GetInvitationCommand command, CancellationToken cancellationToken)
        {
            Guid inviteId = command.Id;

            if (inviteId == Guid.Empty)
            {
                return new CommandResult<YaInvitation>(CommandStatus.BadRequest, null);
            }

            YaInvitation invite = await _dbContext.GetInvitationAsync(e => e.YaInvitationID == inviteId, cancellationToken);

            if (invite == null)
            {
                return new CommandResult<YaInvitation>(CommandStatus.NotFound, null);
            }

            return new CommandResult<YaInvitation>(CommandStatus.Ok, invite);
        }
    }
}
