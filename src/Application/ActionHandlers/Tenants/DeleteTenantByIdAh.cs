using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YA.UserWorker.Application.Features.Tenants.Commands;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class DeleteTenantByIdAh : IDeleteTenantByIdAh
{
    public DeleteTenantByIdAh(ILogger<DeleteTenantByIdAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    private readonly ILogger<DeleteTenantByIdAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;

    public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, CancellationToken cancellationToken)
    {
        ICommandResult result = await _mediator
            .Send(new DeleteTenantByIdCommand(yaTenantId), cancellationToken);

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
