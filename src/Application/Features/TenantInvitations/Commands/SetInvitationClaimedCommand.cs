using AutoMapper;
using MediatR;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.TenantInvitations.Commands;

public class SetInvitationClaimedCommand : IRequest<ICommandResult<EmptyCommandResult>>
{
    public SetInvitationClaimedCommand(Guid id, Guid resultMembershipId)
    {
        Id = id;
        ResultMembershipId = resultMembershipId;
    }

    public Guid Id { get; protected set; }
    public Guid ResultMembershipId { get; protected set; }

    public class DeleteInviteHandler : IRequestHandler<SetInvitationClaimedCommand, ICommandResult<EmptyCommandResult>>
    {
        public DeleteInviteHandler(ILogger<DeleteInviteHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<DeleteInviteHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<EmptyCommandResult>> Handle(SetInvitationClaimedCommand command, CancellationToken cancellationToken)
        {
            Guid invitationId = command.Id;
            Guid resultMembershipId = command.ResultMembershipId;

            if (invitationId == Guid.Empty)
            {
                return new CommandResult<EmptyCommandResult>(CommandStatus.BadRequest, null);
            }

            if (resultMembershipId == Guid.Empty)
            {
                return new CommandResult<EmptyCommandResult>(CommandStatus.BadRequest, null);
            }

            YaInvitation invitation = await _dbContext.GetInvitationAsync(e => e.YaInvitationID == invitationId, cancellationToken);
            Membership membership = await _dbContext.GetMembershipWithUserAsync(e => e.MembershipID == resultMembershipId, cancellationToken);

            if (invitation == null)
            {
                return new CommandResult<EmptyCommandResult>(CommandStatus.NotFound, null);
            }

            if (membership == null)
            {
                return new CommandResult<EmptyCommandResult>(CommandStatus.NotFound, null);
            }

            invitation.SetClaimed(resultMembershipId);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult<EmptyCommandResult>(CommandStatus.Ok, null);
        }
    }
}
