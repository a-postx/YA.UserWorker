using AutoMapper;
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
using YA.TenantWorker.Application.Validators;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.Tenants.Commands
{
    public class UpdateTenantCommand : IRequest<ICommandResult<Tenant>>
    {
        public UpdateTenantCommand(JsonPatchDocument<TenantSm> patch)
        {
            Patch = patch;
        }

        public JsonPatchDocument<TenantSm> Patch { get; protected set; }

        public class UpdateTenantHandler : IRequestHandler<UpdateTenantCommand, ICommandResult<Tenant>>
        {
            public UpdateTenantHandler(ILogger<UpdateTenantHandler> logger,
                IMapper mapper,
                ITenantWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<UpdateTenantHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

            public async Task<ICommandResult<Tenant>> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
            {
                JsonPatchDocument<TenantSm> patch = command.Patch;

                if (patch == null)
                {
                    return new CommandResult<Tenant>(CommandStatus.BadRequest, null);
                }

                Tenant tenant = await _dbContext.GetTenantAsync(cancellationToken);

                if (tenant == null)
                {
                    return new CommandResult<Tenant>(CommandStatus.NotFound, null);
                }

                TenantSm tenantSm = _mapper.Map<TenantSm>(tenant);

                patch.ApplyTo(tenantSm);

                TenantSmValidator validator = new TenantSmValidator();
                ValidationResult validationResult = validator.Validate(tenantSm);

                if (!validationResult.IsValid)
                {
                    return new CommandResult<Tenant>(CommandStatus.ModelInvalid, null, validationResult);
                }

                tenant = (Tenant)_mapper.Map(tenantSm, tenant, typeof(TenantSm), typeof(Tenant));

                _dbContext.UpdateTenant(tenant);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                TenantTm tenantTm = _mapper.Map<TenantTm>(tenant);
                await _messageBus.TenantUpdatedV1Async(tenant.TenantID, tenantTm, cancellationToken);

                return new CommandResult<Tenant>(CommandStatus.Ok, tenant);
            }
        }
    }
}
