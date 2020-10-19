using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.Common;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.Tenants.Commands
{
    public class CreateTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public CreateTenantCommand(string userId, string userEmail)
        {
            UserId = userId;
            UserEmail = userEmail;
        }

        public string UserId { get; protected set; }
        public string UserEmail { get; protected set; }

        public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, ICommandResult<Tenant>>
        {
            public CreateTenantHandler(ILogger<CreateTenantHandler> logger,
                IMapper mapper,
                ITenantWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
                
            }

            private readonly ILogger<CreateTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<Tenant>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
            {
                string userId = command.UserId;
                string userEmail = command.UserEmail;

                Tenant existingTenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                if (existingTenant != null)
                {
                    return new CommandResult<Tenant>(CommandStatuses.UnprocessableEntity, null);
                }


                Guid tenantId = TenantIdGenerator.Create(userId);

                Tenant tenant = new Tenant
                {
                    TenantID = tenantId,
                    TenantName = userEmail,
                    //IsActive = isActive,
                    IsActive = true,
                    TenantType = Core.Entities.TenantTypes.Custom
                };

                Guid defaultPricingTierId = Guid.Parse(SeedData.SeedPricingTierId);
                tenant.PricingTierId = defaultPricingTierId;

                await _dbContext.CreateTenantAsync(tenant, cancellationToken);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.TenantCreatedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

                return new CommandResult<Tenant>(CommandStatuses.Ok, tenant);
            }
        }
    }
}
