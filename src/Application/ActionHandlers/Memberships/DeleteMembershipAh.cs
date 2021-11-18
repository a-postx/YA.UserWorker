using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using YA.UserWorker.Application.Features.Memberships.Commands;
using YA.UserWorker.Application.Features.Memberships.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Memberships;

public class DeleteMembershipAh : IDeleteMembershipAh
{
    public DeleteMembershipAh(ILogger<DeleteMembershipAh> logger,
        IActionContextAccessor actionCtx,
        IMediator mediator,
        IAuthProviderManager authProviderManager)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _authManager = authProviderManager ?? throw new ArgumentNullException(nameof(authProviderManager));
    }

    private readonly ILogger<DeleteMembershipAh> _log;
    private readonly IActionContextAccessor _actionCtx;
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

        string auth0UserId = getResult.Data.User.AuthProvider + "|" + getResult.Data.User.ExternalId;

        //check and delete from auth0
        Guid currentTenantForTargetUser = await _authManager.GetUserTenantAsync(auth0UserId, cancellationToken);

        if (currentTenantForTargetUser == targetTenantId)
        {
            await _authManager.RemoveTenantAsync(auth0UserId, cancellationToken);
        }

        return new NoContentResult();
    }
}
