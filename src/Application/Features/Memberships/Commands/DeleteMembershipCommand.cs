using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Memberships.Commands
{
    public class DeleteMembershipCommand : IRequest<ICommandResult<EmptyCommandResult>>
    {
        public DeleteMembershipCommand(Guid id, Guid tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }

        public Guid Id { get; protected set; }
        public Guid TenantId { get; protected set; }

        public class DeleteMembershipHandler : IRequestHandler<DeleteMembershipCommand, ICommandResult<EmptyCommandResult>>
        {
            public DeleteMembershipHandler(ILogger<DeleteMembershipHandler> logger,
                IUserWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<DeleteMembershipHandler> _log;
            private readonly IUserWorkerDbContext _dbContext;

            public async Task<ICommandResult<EmptyCommandResult>> Handle(DeleteMembershipCommand command, CancellationToken cancellationToken)
            {
                Guid membershipId = command.Id;
                Guid tenantId = command.TenantId;

                if (membershipId == Guid.Empty)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatus.BadRequest, null);
                }

                if (tenantId == Guid.Empty)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatus.BadRequest, null);
                }

                Membership yaMembership = await _dbContext
                    .GetMembershipWithUserAsync(e => e.MembershipID == membershipId && e.TenantID == tenantId, cancellationToken);

                if (yaMembership == null)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatus.NotFound, null);
                }

                _dbContext.DeleteMembership(yaMembership);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                return new CommandResult<EmptyCommandResult>(CommandStatus.Ok, null);
            }
        }
    }
}
