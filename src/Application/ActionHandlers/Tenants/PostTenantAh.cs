using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using YA.Common.Constants;
using YA.TenantWorker.Application.CommandsAndQueries.Tenants.Commands;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class PostTenantAh : IPostTenantAh
    {
        public PostTenantAh(ILogger<PostTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<PostTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            ClaimsPrincipal user = _actionCtx.ActionContext.HttpContext.User;

            string userId = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.uid)?.Value;
            string userEmail = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email)?.Value;
            ////string emailVerified = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email_verified)?.Value;
            //необходимо создать процесс верификации почты 
            ////bool gotEmailVerification = bool.TryParse(emailVerified, out bool verificationResult);
            ////bool isActive = gotEmailVerification ? verificationResult : false;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return new BadRequestResult();
            }

            ICommandResult<TenantVm> result = await _mediator
                .Send(new PostTenantCommand(userId, userEmail), cancellationToken);

            switch (result.Status)
            {
                case CommandStatuses.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatuses.UnprocessableEntity:
                    return new UnprocessableEntityResult();
                case CommandStatuses.Ok:
                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                    return new CreatedAtRouteResult(RouteNames.GetTenant, new { TenantId = result.Data.TenantId, TenantName = result.Data.TenantName }, result.Data);
            }
        }
    }
}
