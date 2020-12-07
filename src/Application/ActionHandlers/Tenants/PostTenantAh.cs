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
using YA.TenantWorker.Application.Features.Tenants.Commands;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using Delobytes.Mapper;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class PostTenantAh : IPostTenantAh
    {
        public PostTenantAh(ILogger<PostTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<PostTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            ClaimsPrincipal user = _actionCtx.ActionContext.HttpContext.User;

            //делать запрос на /userinfo, чтобы убрать лишние удостоверения из токена доступа 
            string tenantId = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.tid)?.Value;
            string userId = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.uid)?.Value;
            string userName = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.name)?.Value;
            string userEmail = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email)?.Value;
            
            ////string emailVerified = user.Claims.FirstOrDefault(claim => claim.Type == YaClaimNames.email_verified)?.Value;
            //bool isActive;
            //if (bool.TryParse(emailVerified, out bool verificationResult))
            //{
            //    isActive = verificationResult;
            //}

            if (string.IsNullOrEmpty(tenantId))
            {
                return new BadRequestResult();
            }

            ICommandResult<Tenant> result = await _mediator
                .Send(new CreateTenantCommand(tenantId, userId, userName, userEmail), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.UnprocessableEntity:
                    return new UnprocessableEntityResult();
                case CommandStatus.Ok:
                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                    TenantVm tenantVm = _tenantVmMapper.Map(result.Data);

                    return new CreatedAtRouteResult(RouteNames.GetTenant, null, tenantVm);
            }
        }
    }
}
