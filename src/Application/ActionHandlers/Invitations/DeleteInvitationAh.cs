using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features;
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Application.ActionHandlers.Invitations
{
    public class DeleteInvitationAh : IDeleteInvitationAh
    {
        public DeleteInvitationAh(ILogger<DeleteInvitationAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IRuntimeContextAccessor runtimeCtx)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _runtimeCtx = runtimeCtx ?? throw new ArgumentNullException(nameof(runtimeCtx));
        }

        private readonly ILogger<DeleteInvitationAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IRuntimeContextAccessor _runtimeCtx;

        public async Task<IActionResult> ExecuteAsync(Guid yaInviteId, CancellationToken cancellationToken)
        {
            Guid tenantId = _runtimeCtx.GetTenantId();

            ICommandResult<EmptyCommandResult> result = await _mediator
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
}
