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
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Invitations
{
    public class PostInvitationAh : IPostInvitationAh
    {
        public PostInvitationAh(ILogger<PostInvitationAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IMapper mapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private readonly ILogger<PostInvitationAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public async Task<IActionResult> ExecuteAsync(InvitationSm inviteSm, CancellationToken cancellationToken)
        {
            ICommandResult<YaInvitation> result = await _mediator
                .Send(new CreateInvitationCommand(inviteSm.Email, inviteSm.AccessType, inviteSm.InvitedBy), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.Ok:
                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                    InvitationVm invitationVm = _mapper.Map<InvitationVm>(result.Data);

                    return new CreatedAtRouteResult(RouteNames.PostTenantInvitation, new { }, invitationVm);
            }
        }
    }
}
