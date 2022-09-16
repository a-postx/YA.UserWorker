using System.Globalization;
using AutoMapper;
using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.TenantInvitations.Queries;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Invitations;

public class GetInvitationAh : IGetInvitationAh
{
    public GetInvitationAh(ILogger<GetInvitationAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IMapper mapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private readonly ILogger<GetInvitationAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
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
                if (_httpCtx.HttpContext
                    .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                _httpCtx.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                InvitationVm inviteVm = _mapper.Map<InvitationVm>(result.Data);

                return new OkObjectResult(inviteVm);
        }
    }
}
