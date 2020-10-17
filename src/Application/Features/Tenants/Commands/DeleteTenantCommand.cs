﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.Tenants.Commands
{
    public class DeleteTenantCommand : IRequest<ICommandResult<EmptyCommandResult>>
    {
        public class DeleteTenantHandler : IRequestHandler<DeleteTenantCommand, ICommandResult<EmptyCommandResult>>
        {
            public DeleteTenantHandler(ILogger<DeleteTenantHandler> logger,
                IMapper mapper,
                ITenantWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<DeleteTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<EmptyCommandResult>> Handle(DeleteTenantCommand command, CancellationToken cancellationToken)
            {

                Tenant tenant = await _dbContext.GetTenantAsync(cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<EmptyCommandResult>(CommandStatuses.NotFound, null);
                }

                _dbContext.DeleteTenant(tenant);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.TenantDeletedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

                return new CommandResult<EmptyCommandResult>(CommandStatuses.Ok, null);
            }
        }
    }
}