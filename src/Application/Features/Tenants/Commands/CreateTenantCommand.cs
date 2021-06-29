using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Commands
{
    public class CreateTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public CreateTenantCommand(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }

        public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, ICommandResult<Tenant>>
        {
            public CreateTenantHandler(ILogger<CreateTenantHandler> logger,
                IMapper mapper,
                IUserWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<CreateTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly IUserWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<Tenant>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
            {
                string name = command.Name;

                //дописать: проверка если тенант такого пользователя уже был удалён
                //User existingUser = await _dbContext.GetTenantAsync(authId, userId, cancellationToken);

                //if (existingUser != null)
                //{
                //    return new CommandResult<User>(CommandStatus.UnprocessableEntity, null);
                //}

                Tenant tenant = new Tenant
                {
                    Name = string.IsNullOrEmpty(name) ? "<неизвестный>" : name,
                    Status = YaTenantStatus.New,
                    Type = YaTenantType.Custom
                };

                Guid defaultPricingTierId = Guid.Parse(SeedData.SeedPricingTierId);

                tenant.PricingTierId = defaultPricingTierId;
                tenant.PricingTierActivatedDateTime = DateTime.UtcNow;

                await _dbContext.CreateTenantAsync(tenant, cancellationToken);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.TenantCreatedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

                return new CommandResult<Tenant>(CommandStatus.Ok, tenant);
            }
        }
    }
}
