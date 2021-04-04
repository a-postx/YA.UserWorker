using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.PricingTiers.Queries
{
    public class GetPricingTierCommand : IRequest<ICommandResult<PricingTier>>
    {
        public class GetPricingTierHandler : IRequestHandler<GetPricingTierCommand, ICommandResult<PricingTier>>
        {
            public GetPricingTierHandler(ILogger<GetPricingTierHandler> logger,
                IUserWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<GetPricingTierHandler> _log;
            private readonly IUserWorkerDbContext _dbContext;

            public async Task<ICommandResult<PricingTier>> Handle(GetPricingTierCommand command, CancellationToken cancellationToken)
            {
                await Task.Delay(100, cancellationToken);
                //Tenant tenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                return new CommandResult<PricingTier>(CommandStatus.NotFound, null);
            }
        }
    }
}
