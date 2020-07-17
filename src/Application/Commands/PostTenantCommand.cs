using AutoMapper;
using Delobytes.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class PostTenantCommand : IPostTenantCommand
    {
        public PostTenantCommand(ILogger<PostTenantCommand> logger,
            IMapper mapper,
            IActionContextAccessor actionContextAccessor,
            ITenantWorkerDbContext dbContext,
            IMessageBus messageBus,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<PostTenantCommand> _log;
        private readonly IMapper _mapper;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ITenantWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            ClaimsPrincipal user = _actionContextAccessor.ActionContext.HttpContext.User;

            string userId = user.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.uid)?.Value;
            string userEmail = user.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.email)?.Value;
            string emailVerified = user.Claims.FirstOrDefault(claim => claim.Type == CustomClaimNames.email_verified)?.Value;

            bool gotEmailVerification = bool.TryParse(emailVerified, out bool verificationResult);
            bool isActive = gotEmailVerification ? verificationResult : false;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return new BadRequestResult();
            }

            Tenant existingTenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

            if (existingTenant != null)
            {
                return new UnprocessableEntityResult();
            }

            Guid tenantId = TenantIdGenerator.Create(userId);

            Tenant tenant = new Tenant
            {
                TenantID = tenantId,
                TenantName = userEmail,
                IsActive = isActive,
                TenantType = TenantTypes.Custom
            };

            Guid defaultPricingTierId = Guid.Parse(SeedData.SeedPricingTierId);
            tenant.PricingTierId = defaultPricingTierId;

            await _dbContext.CreateTenantAsync(tenant, cancellationToken);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            await _messageBus.TenantCreatedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

            TenantVm tenantVm = _tenantVmMapper.Map(tenant);

            return new CreatedAtRouteResult(RouteNames.GetTenant, new { TenantId = tenantVm.TenantId, TenantName = tenantVm.TenantName }, tenantVm);
        }
    }
}
