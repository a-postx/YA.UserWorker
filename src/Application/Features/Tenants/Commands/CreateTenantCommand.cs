using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.Tenants.Commands
{
    public class CreateTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public CreateTenantCommand(string tenantId, string userId, string userName, string userEmail)
        {
            TenantId = tenantId;
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
        }

        public string TenantId { get; protected set; }
        public string UserId { get; protected set; }
        public string UserName { get; protected set; }
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
                string tenantId = command.TenantId;
                string userId = command.UserId;
                string userName = command.UserName;
                string userEmail = command.UserEmail;

                Tenant existingTenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                if (existingTenant != null)
                {
                    return new CommandResult<Tenant>(CommandStatuses.UnprocessableEntity, null);
                }

                bool tenantParsed = Guid.TryParse(tenantId, out Guid tenantIdGuid);

                if (!tenantParsed)
                {
                    return new CommandResult<Tenant>(CommandStatuses.UnprocessableEntity, null);
                }

                string[] tenantProps = userId.Split('|');
                string provider = tenantProps[0];
                string externalId = tenantProps[1];

                Tenant tenant = new Tenant
                {
                    TenantID = tenantIdGuid,
                    Name = userName,
                    Email = userEmail,
                    AuthProvider = provider,
                    ExternalId = externalId,
                    //IsActive = isActive,
                    Status = Core.Entities.TenantStatuses.New,
                    Type = Core.Entities.TenantTypes.Custom
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
