using System.Globalization;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features.TenantInvitations.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Invitations;

public class GetInvitationAh : IGetInvitationAh
{
    public GetInvitationAh(ILogger<GetInvitationAh> logger,
        IActionContextAccessor actionCtx,
        IMediator mediator,
        IMapper mapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private readonly ILogger<GetInvitationAh> _log;
    private readonly IActionContextAccessor _actionCtx;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public async Task<IActionResult> ExecuteAsync(Guid yaTenantInvitationId, CancellationToken cancellationToken)
    {
        ICommandResult<YaInvitation> result = await _mediator
            .Send(new GetInvitationCommand(yaTenantInvitationId), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.BadRequest:
                return new BadRequestResult();
            case CommandStatus.Ok:
                if (_actionCtx.ActionContext.HttpContext
                    .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                _actionCtx.ActionContext.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                InvitationVm inviteVm = _mapper.Map<InvitationVm>(result.Data);

                return new OkObjectResult(inviteVm);
        }
    }
}
