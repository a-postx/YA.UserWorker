using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.PricingTiers.Queries
{
    public class GetPricingTierCommand : IRequest<ICommandResult<PricingTierTm>>
    {
        public class GetPricingTierHandler : IRequestHandler<GetPricingTierCommand, ICommandResult<PricingTierTm>>
        {
            public GetPricingTierHandler(ILogger<GetPricingTierHandler> logger,
                ITenantWorkerDbContext dbContext,
                IMapper mapper)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            private readonly ILogger<GetPricingTierHandler> _log;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMapper _mapper;

            public async Task<ICommandResult<PricingTierTm>> Handle(GetPricingTierCommand command, CancellationToken cancellationToken)
            {
                Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                PricingTierTm pricingTierTm = _mapper.Map<PricingTierTm>(tenant.PricingTier);

                return new CommandResult<PricingTierTm>(CommandStatuses.Ok, pricingTierTm);
            }
        }
    }
}
