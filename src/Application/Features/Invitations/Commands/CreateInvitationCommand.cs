using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Invitations.Commands
{
    public class CreateInvitationCommand : IRequest<ICommandResult<YaInvitation>>
    {
        public CreateInvitationCommand(string email, MembershipAccessType accessType, string inviterEmail)
        {
            Email = email;
            AccessType = accessType;
            InviterEmail = inviterEmail;
        }

        public string Email { get; protected set; }
        public MembershipAccessType AccessType { get; protected set; }
        public string InviterEmail { get; protected set; }

        public class CreateInviteHandler : IRequestHandler<CreateInvitationCommand, ICommandResult<YaInvitation>>
        {
            public CreateInviteHandler(ILogger<CreateInviteHandler> logger,
                IRuntimeContextAccessor runtimeCtx,
                IMapper mapper,
                IUserWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<CreateInviteHandler> _log;
            private readonly IRuntimeContextAccessor _runtimeCtx;
            private readonly IMapper _mapper;
            private readonly IUserWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<YaInvitation>> Handle(CreateInvitationCommand command, CancellationToken cancellationToken)
            {
                string email = command.Email;
                string inviterEmail = command.InviterEmail;
                MembershipAccessType accessType = command.AccessType;

                Guid tenantId = _runtimeCtx.GetTenantId();

                YaInvitation yaInvite = new YaInvitation
                {
                    Email = email,
                    AccessType = (YaMembershipAccessType)accessType,
                    InvitedBy = inviterEmail,
                    TenantId = tenantId,
                    ExpirationDate = DateTime.UtcNow.AddMonths(1)
                };
                
                await _dbContext.CreateInvitationAsync(yaInvite, cancellationToken);

                await _dbContext.ApplyChangesAsync(cancellationToken);

                //InviteTm inviteTm = _mapper.Map<InviteTm>(yaInvite);
                //await _messageBus.InviteCreatedV1Async(yaInvite.YaInviteID, inviteTm, cancellationToken);

                return new CommandResult<YaInvitation>(CommandStatus.Ok, yaInvite);
            }
        }
    }
}
