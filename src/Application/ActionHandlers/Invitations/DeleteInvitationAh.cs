using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Application.ActionHandlers.Invitations;

public class DeleteInvitationAh : IDeleteInvitationAh
{
    public DeleteInvitationAh(ILogger<DeleteInvitationAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IRuntimeContextAccessor runtimeCtx)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
    }

    private readonly ILogger<DeleteInvitationAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;
    private readonly IRuntimeContextAccessor _runtimeCtx;

    public async Task<IActionResult> ExecuteAsync(Guid yaInviteId, CancellationToken cancellationToken)
    {
        Guid tenantId = _runtimeCtx.GetTenantId();

        ICommandResult result = await _mediator
            .Send(new DeleteInvitationCommand(yaInviteId, tenantId), cancellationToken);

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
                return new NoContentResult();
        }
    }
}
