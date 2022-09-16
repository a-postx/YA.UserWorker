using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using YA.UserWorker.Application.Features.Memberships.Commands;
using YA.UserWorker.Application.Features.Memberships.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Memberships;

public class DeleteMembershipAh : IDeleteMembershipAh
{
    public DeleteMembershipAh(ILogger<DeleteMembershipAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IAuthProviderManager authProviderManager)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _authManager = authProviderManager ?? throw new ArgumentNullException(nameof(authProviderManager));
    }

    private readonly ILogger<DeleteMembershipAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;
    private readonly IAuthProviderManager _authManager;

    public async Task<IActionResult> ExecuteAsync(Guid yaMembershipId, CancellationToken cancellationToken)
    {
        ICommandResult<Membership> getResult = await _mediator
            .Send(new GetMembershipCommand(yaMembershipId), cancellationToken);

        switch (getResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(getResult.Status), getResult.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.BadRequest:
                return new BadRequestResult();
            case CommandStatus.Ok:
                break;
        }

        Guid targetTenantId = getResult.Data.TenantID;

        if (getResult.Data.User is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        ICommandResult deleteResult = await _mediator
            .Send(new DeleteMembershipCommand(yaMembershipId, targetTenantId), cancellationToken);

        switch (deleteResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(deleteResult.Status), deleteResult.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.BadRequest:
                return new BadRequestResult();
            case CommandStatus.Ok:
                break;
        }

        string userId = getResult.Data.User.ExternalId;

        Guid currentTenantForTargetUser = await _authManager.GetUserTenantAsync(userId, cancellationToken);

        if (currentTenantForTargetUser == targetTenantId)
        {
            await _authManager.RemoveTenantAsync(userId, cancellationToken);
        }

        return new NoContentResult();
    }
}
