using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.CommandsAndQueries;
using YA.TenantWorker.Application.CommandsAndQueries.Tenants.Commands;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class DeleteTenantByIdAh : IDeleteTenantByIdAh
    {
        public DeleteTenantByIdAh(ILogger<DeleteTenantByIdAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<DeleteTenantByIdAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;

        public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, CancellationToken cancellationToken)
        {
            ICommandResult<EmptyCommandResult> result = await _mediator
                .Send(new DeleteTenantByIdCommand(yaTenantId), cancellationToken);

            switch (result.Status)
            {
                case CommandStatuses.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatuses.NotFound:
                    return new NotFoundResult();
                case CommandStatuses.BadRequest:
                    return new BadRequestResult();
                case CommandStatuses.Ok:
                    return new NoContentResult();
            }
        }
    }
}
