using System.Globalization;
using AutoMapper;
using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Invitations;

public class PostInvitationAh : IPostInvitationAh
{
    public PostInvitationAh(ILogger<PostInvitationAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IMapper mapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private readonly ILogger<PostInvitationAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
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
                _httpCtx.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                InvitationVm invitationVm = _mapper.Map<InvitationVm>(result.Data);

                return new CreatedAtRouteResult(RouteNames.PostTenantInvitation, new { }, invitationVm);
        }
    }
}
