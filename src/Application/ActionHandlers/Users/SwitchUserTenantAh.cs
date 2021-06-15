using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public class SwitchUserTenantAh : ISwitchUserTenantAh
    {
        public SwitchUserTenantAh(ILogger<SwitchUserTenantAh> logger,
            IActionContextAccessor actionCtx,
            IRuntimeContextAccessor runtimeContext,
            IAuthProviderManager authProviderManager,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
            _authProviderManager = authProviderManager ?? throw new ArgumentNullException(nameof(authProviderManager));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<SwitchUserTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
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
                    if (result.Data.Tenants.Where(e => e.TenantID == targetTenantId).Any())
                    {
                        Membership membership = result.Data.Memberships.Where(e => e.TenantID == targetTenantId).FirstOrDefault();

                        if (membership is not null)
                        {
                            await _authProviderManager
                                .SetTenantAsync(authId + "|" + userId, targetTenantId, membership.AccessType, cancellationToken);

                            _log.LogInformation("User {UserId} has updated with tenant {TenantId}", userId, targetTenantId);

                            _actionCtx.ActionContext.HttpContext
                                .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                            return new OkResult();
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    else
                    {
                        return new NotFoundResult();
                    }
            }
        }
    }
}
