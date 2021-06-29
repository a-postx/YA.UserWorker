using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Application.Features;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using System.Linq;
using YA.UserWorker.Core.Entities;
using YA.UserWorker.Application.Features.Users.Queries;
using System.Collections.Generic;

namespace YA.UserWorker.Application.ActionHandlers.Tenants
{
    public class DeleteTenantAh : IDeleteTenantAh
    {
        public DeleteTenantAh(ILogger<DeleteTenantAh> logger,
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

        private readonly ILogger<DeleteTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
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

            ICommandResult<EmptyCommandResult> deleteTenantResult = await _mediator
                .Send(new DeleteTenantCommand(tenantId), cancellationToken);

            switch (deleteTenantResult.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(deleteTenantResult.Status), deleteTenantResult.Status, null);
                case CommandStatus.NotFound:
                    return new NotFoundResult();
                case CommandStatus.Ok:
                    await _authProviderManager.RemoveTenantAsync(authId + "|" + userId, cancellationToken);

                    return new NoContentResult();
            }
        }
    }
}
