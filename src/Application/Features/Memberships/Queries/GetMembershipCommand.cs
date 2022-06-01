using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Memberships.Queries;

public class GetMembershipCommand : IRequest<ICommandResult<Membership>>
{
    public GetMembershipCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }

    public class GetMembershipHandler : IRequestHandler<GetMembershipCommand, ICommandResult<Membership>>
    {
        public GetMembershipHandler(ILogger<GetMembershipHandler> logger,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<GetMembershipHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<Membership>> Handle(GetMembershipCommand command, CancellationToken cancellationToken)
        {
            Guid membershipId = command.Id; 

            if (membershipId == Guid.Empty)
            {
                return new CommandResult<Membership>(CommandStatus.BadRequest, null);
            }

            Membership membership = await _dbContext.GetMembershipWithUserAsync(e => e.MembershipID == membershipId, cancellationToken);

            if (membership == null)
            {
                return new CommandResult<Membership>(CommandStatus.NotFound, null);
            }

            return new CommandResult<Membership>(CommandStatus.Ok, membership);
        }
    }
}
