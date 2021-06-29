using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Memberships.Commands
{
    public class CreateMembershipCommand : IRequest<ICommandResult<Membership>>
    {
        public CreateMembershipCommand(Guid userId, Guid tenantId, YaMembershipAccessType accessType)
        {
            UserId = userId;
            TenantId = tenantId;
            AccessType = accessType;
        }

        public Guid UserId { get; protected set; }
        public Guid TenantId { get; protected set; }
        public YaMembershipAccessType AccessType { get; protected set; }

        public class CreateMembershipHandler : IRequestHandler<CreateMembershipCommand, ICommandResult<Membership>>
        {
            public CreateMembershipHandler(ILogger<CreateMembershipHandler> logger,
                IMapper mapper,
                IUserWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<CreateMembershipHandler> _log;
            private readonly IMapper _mapper;
            private readonly IUserWorkerDbContext _dbContext;

            public async Task<ICommandResult<Membership>> Handle(CreateMembershipCommand command, CancellationToken cancellationToken)
            {
                Guid userId = command.UserId;
                Guid tenantId = command.TenantId;
                YaMembershipAccessType accessType = command.AccessType;

                Membership existingMembership = await _dbContext
                    .GetMembershipAsync(e => e.TenantID == tenantId && e.UserID == userId, cancellationToken);

                if (existingMembership != null)
                {
                    return new CommandResult<Membership>(CommandStatus.UnprocessableEntity, null);
                }

                Membership membership = new Membership
                {
                    UserID = userId,
                    TenantID = tenantId,
                    AccessType = accessType
                };

                await _dbContext.CreateMembershipAsync(membership, cancellationToken);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                return new CommandResult<Membership>(CommandStatus.Ok, membership);
            }
        }
    }
}
