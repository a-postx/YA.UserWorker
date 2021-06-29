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
        public DeleteMembershipCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; protected set; }

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

                if (membershipId == Guid.Empty)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatus.BadRequest, null);
                }

                //доделать: нет автофильтра по арендатору
                Membership yaMembership = await _dbContext
                    .GetMembershipAsync(e => e.MembershipID == membershipId, cancellationToken);

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
