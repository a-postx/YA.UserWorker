using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features;
using YA.UserWorker.Application.Features.Invitations.Commands;
using YA.UserWorker.Application.Interfaces;

namespace YA.UserWorker.Application.ActionHandlers.Invitations
{
    public class DeleteInvitationAh : IDeleteInvitationAh
    {
        public DeleteInvitationAh(ILogger<DeleteInvitationAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<DeleteInvitationAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;

        public async Task<IActionResult> ExecuteAsync(Guid yaInviteId, CancellationToken cancellationToken)
        {
            ICommandResult<EmptyCommandResult> result = await _mediator
                .Send(new DeleteInvitationCommand(yaInviteId), cancellationToken);

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
