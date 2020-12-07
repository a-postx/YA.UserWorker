using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.PricingTiers.Queries
{
    public class GetPricingTierCommand : IRequest<ICommandResult<PricingTier>>
    {
        public class GetPricingTierHandler : IRequestHandler<GetPricingTierCommand, ICommandResult<PricingTier>>
        {
            public GetPricingTierHandler(ILogger<GetPricingTierHandler> logger,
                ITenantWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<GetPricingTierHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;

            public async Task<ICommandResult<PricingTier>> Handle(GetPricingTierCommand command, CancellationToken cancellationToken)
            {
                Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                return new CommandResult<PricingTier>(CommandStatus.Ok, tenant.PricingTier);
            }
        }
    }
}
