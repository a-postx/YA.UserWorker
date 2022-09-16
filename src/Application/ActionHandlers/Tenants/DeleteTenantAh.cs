using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public class DeleteTenantAh : IDeleteTenantAh
{
    public DeleteTenantAh(ILogger<DeleteTenantAh> logger,
        IHttpContextAccessor httpCtx,
        IRuntimeContextAccessor runtimeContext,
        IAuthProviderManager authProviderManager,
        IMediator mediator)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        _authProviderManager = authProviderManager ?? throw new ArgumentNullException(nameof(authProviderManager));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    private readonly ILogger<DeleteTenantAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IAuthProviderManager _authProviderManager;
    private readonly IMediator _mediator;

    public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        (string authId, string userId) = _runtimeCtx.GetUserIdentifiers();

        ICollection<Membership> memberships;

        ICommandResult<User> getUserResult = await _mediator
            .Send(new GetUserCommand(authId, userId), cancellationToken);

        switch (getUserResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(getUserResult.Status), getUserResult.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                memberships = getUserResult.Data.Memberships;
                break;
        }

        Guid tenantId = _runtimeCtx.GetTenantId();
            
        Membership userMembership = memberships.Where(e => e.TenantID == tenantId).FirstOrDefault();

        if (userMembership == null || userMembership.AccessType != YaMembershipAccessType.Owner)
        {
            return new ForbidResult();
        }

        ICommandResult deleteTenantResult = await _mediator
            .Send(new DeleteTenantCommand(tenantId), cancellationToken);

        switch (deleteTenantResult.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(deleteTenantResult.Status), deleteTenantResult.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                await _authProviderManager.RemoveTenantAsync(userId, cancellationToken);

                return new NoContentResult();
        }
    }
}
