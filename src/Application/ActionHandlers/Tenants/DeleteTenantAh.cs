using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Features;
using YA.TenantWorker.Application.Features.Tenants.Commands;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class DeleteTenantAh : IDeleteTenantAh
    {
        public DeleteTenantAh(ILogger<DeleteTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<DeleteTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            ICommandResult<EmptyCommandResult> result = await _mediator
                .Send(new DeleteTenantCommand(), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.NotFound:
                    return new NotFoundResult();
                case CommandStatus.Ok:
                    return new NoContentResult();
            }
        }
    }
}
