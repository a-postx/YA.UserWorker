﻿using AutoMapper;
using Delobytes.Mapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.Common;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.CommandsAndQueries.Tenants.Commands
{
    public class PostTenantCommand : IRequest<ICommandResult<TenantVm>>
    {
        public PostTenantCommand(string userId, string userEmail)
        {
            UserId = userId;
            UserEmail = userEmail;
        }

        public string UserId { get; protected set; }
        public string UserEmail { get; protected set; }

        public class PostTenantHandler : IRequestHandler<PostTenantCommand, ICommandResult<TenantVm>>
        {
            public PostTenantHandler(ILogger<PostTenantHandler> logger,
                IMapper mapper,
                ITenantWorkerDbContext dbContext,
                IMessageBus messageBus,
                IMapper<Tenant, TenantVm> tenantVmMapper)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
                _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
            }

            private readonly ILogger<PostTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;
            private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

            public async Task<ICommandResult<TenantVm>> Handle(PostTenantCommand command, CancellationToken cancellationToken)
            {
                string userId = command.UserId;
                string userEmail = command.UserEmail;

                Tenant existingTenant = await _dbContext.GetTenantWithPricingTierAsync(cancellationToken);

                if (existingTenant != null)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.UnprocessableEntity, null);
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

                TenantVm tenantVm = _tenantVmMapper.Map(tenant);

                return new CommandResult<TenantVm>(CommandStatuses.Ok, tenantVm);
            }
        }
    }
}
