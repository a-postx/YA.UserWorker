using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features;
using YA.UserWorker.Application.Features.Invitations.Commands;
using YA.UserWorker.Application.Features.Invitations.Queries;
using YA.UserWorker.Application.Features.Memberships.Commands;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Memberships
{
    public class PostMembershipAh : IPostMembershipAh
    {
        public PostMembershipAh(ILogger<PostMembershipAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IMapper mapper,
            IRuntimeContextAccessor runtimeCtx)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
        }

        private readonly ILogger<PostMembershipAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IRuntimeContextAccessor _runtimeCtx;

        public async Task<IActionResult> ExecuteAsync(Guid yaTenantInvitationId, CancellationToken cancellationToken)
        {
            ICommandResult<YaInvitation> inviteResult = await _mediator
                .Send(new GetInvitationCommand(yaTenantInvitationId), cancellationToken);

            switch (inviteResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(inviteResult.Status), inviteResult.Status, null);
                case CommandStatus.NotFound:
                    return new NotFoundResult();
                case CommandStatus.BadRequest:
                    return new BadRequestResult();
                case CommandStatus.Ok:
                    break;
            }

            //доделать: возвращать детали проблемы
            if (inviteResult.Data is null)
            {
                return new BadRequestResult();
            }

            YaInvitation yaInvite = inviteResult.Data;

            if (yaInvite.ExpirationDate.HasValue && yaInvite.ExpirationDate.Value < DateTime.UtcNow)
            {
                return new BadRequestResult();
            }

            if (yaInvite.Claimed)
            {
                return new BadRequestResult();
            }

            (string authId, string userId) = _runtimeCtx.GetUserIdentifiers();

            ICommandResult<User> userResult = await _mediator
                .Send(new GetUserCommand(authId, userId), cancellationToken);

            switch (userResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(userResult.Status), userResult.Status, null);
                case CommandStatus.NotFound:
                    throw new InvalidOperationException("User is not found.");
                case CommandStatus.Ok:
                    break;
            }

            if (userResult.Data is null)
            {
                return new BadRequestResult();
            }

            ICommandResult<Membership> membershipResult = await _mediator
                .Send(new CreateMembershipCommand(userResult.Data.UserID, yaInvite.TenantId, yaInvite.AccessType), cancellationToken);

            switch (membershipResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(membershipResult.Status), membershipResult.Status, null);
                case CommandStatus.UnprocessableEntity: //пользователь уже является членом этого арендатора
                    return new BadRequestResult();
                case CommandStatus.Ok:
                    break;
            }

            if (membershipResult.Data is null)
            {
                return new BadRequestResult();
            }

            ICommandResult<EmptyCommandResult> invitationClaimedResult = await _mediator
                .Send(new SetInvitationClaimedCommand(yaInvite.YaInvitationID, membershipResult.Data.MembershipID), cancellationToken);

            switch (invitationClaimedResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(invitationClaimedResult.Status), invitationClaimedResult.Status, "Unexpected result on setting invitation claimed");
                case CommandStatus.Ok:
                    break;
            }

            _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, membershipResult.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

            MembershipVm membershipVm = _mapper.Map<MembershipVm>(membershipResult.Data);

            return new CreatedAtRouteResult(RouteNames.PostTenantMembership, new { }, membershipVm);
        }
    }
}
