using System.Globalization;
using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users;

public class SwitchUserTenantAh : ISwitchUserTenantAh
{
    public SwitchUserTenantAh(ILogger<SwitchUserTenantAh> logger,
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

    private readonly ILogger<SwitchUserTenantAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IRuntimeContextAccessor _runtimeCtx;
    private readonly IAuthProviderManager _authProviderManager;
    private readonly IMediator _mediator;

    public async Task<IActionResult> ExecuteAsync(Guid targetTenantId, CancellationToken cancellationToken)
    {
        (string authId, string userId) = _runtimeCtx.GetUserIdentifiers();

        ICommandResult<User> result = await _mediator
            .Send(new GetUserCommand(authId, userId), cancellationToken);

        switch (result.Status)
        {
            case CommandStatus.Unknown:
            default:
                throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
            case CommandStatus.NotFound:
                return new NotFoundResult();
            case CommandStatus.Ok:
                break;
        }

        if (result.Data is null)
        {
            return new BadRequestResult();
        }

        User user = result.Data;

        if (!user.Tenants.Where(e => e.TenantID == targetTenantId).Any())
        {
            return new BadRequestResult();
        }

        Membership membership = user.Memberships.Where(e => e.TenantID == targetTenantId).FirstOrDefault();

        if (membership is null)
        {
            return new BadRequestResult();
        }

        await _authProviderManager
                .SetTenantAsync(userId, targetTenantId, membership.AccessType, cancellationToken);

        _log.LogInformation("User {UserId} has been updated with tenant {TenantId}", userId, targetTenantId);

        _httpCtx.HttpContext
            .Response.Headers.Add(HeaderNames.LastModified, user.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

        return new OkResult();
    }
}
