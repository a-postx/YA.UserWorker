﻿using AutoMapper;
using Delobytes.Mapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Application.Validators;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.CommandsAndQueries.Tenants.Commands
{
    public class PatchTenantByIdCommand : IRequest<ICommandResult<TenantVm>>
    {
        public PatchTenantByIdCommand(Guid tenantId, JsonPatchDocument<TenantSm> patch)
        {
            TenantId = tenantId;
            Patch = patch;
        }

        public Guid TenantId { get; protected set; }
        public JsonPatchDocument<TenantSm> Patch { get; protected set; }

        public class PatchTenantByIdHandler : IRequestHandler<PatchTenantByIdCommand, ICommandResult<TenantVm>>
        {
            public PatchTenantByIdHandler(ILogger<PatchTenantByIdHandler> logger,
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

            private readonly ILogger<PatchTenantByIdHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;
            private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

            public async Task<ICommandResult<TenantVm>> Handle(PatchTenantByIdCommand command, CancellationToken cancellationToken)
            {
                Guid tenantId = command.TenantId;
                JsonPatchDocument<TenantSm> patch = command.Patch;

                if (tenantId == Guid.Empty || patch == null)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.BadRequest, null);
                }

                Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.NotFound, null);
                }

                TenantSm tenantSm = _mapper.Map<TenantSm>(tenant);

                patch.ApplyTo(tenantSm);

                TenantSmValidator validator = new TenantSmValidator();
                ValidationResult validationResult = validator.Validate(tenantSm);

                if (!validationResult.IsValid)
                {
                    return new CommandResult<TenantVm>(CommandStatuses.ModelInvalid, null, validationResult);
                }

                tenant = (Tenant)_mapper.Map(tenantSm, tenant, typeof(TenantSm), typeof(Tenant));

                _dbContext.UpdateTenant(tenant);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                TenantTm tenantTm = _mapper.Map<TenantTm>(tenant);
                await _messageBus.TenantUpdatedV1Async(tenant.TenantID, tenantTm, cancellationToken);

                TenantVm tenantVm = _tenantVmMapper.Map(tenant);

                return new CommandResult<TenantVm>(CommandStatuses.Ok, tenantVm);
            }
        }
    }
}
