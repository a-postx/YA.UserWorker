using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Memberships.Commands;

public class DeleteMembershipCommand : IRequest<ICommandResult>
{
    public DeleteMembershipCommand(Guid id, Guid tenantId)
    {
        Id = id;
        TenantId = tenantId;
    }

    public Guid Id { get; protected set; }
    public Guid TenantId { get; protected set; }

    public class DeleteMembershipHandler : IRequestHandler<DeleteMembershipCommand, ICommandResult>
    {
        public DeleteMembershipHandler(ILogger<DeleteMembershipHandler> logger,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<DeleteMembershipHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult> Handle(DeleteMembershipCommand command, CancellationToken cancellationToken)
        {
            Guid membershipId = command.Id;
            Guid tenantId = command.TenantId;

            if (membershipId == Guid.Empty)
            {
                return new CommandResult(CommandStatus.BadRequest);
            }

            if (tenantId == Guid.Empty)
            {
                return new CommandResult(CommandStatus.BadRequest);
            }

            Membership yaMembership = await _dbContext
                .GetMembershipWithUserAsync(e => e.MembershipID == membershipId && e.TenantID == tenantId, cancellationToken);

            if (yaMembership == null)
            {
                return new CommandResult(CommandStatus.NotFound);
            }

            _dbContext.DeleteMembership(yaMembership);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult(CommandStatus.Ok);
        }
    }
}
