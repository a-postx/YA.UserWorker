using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Commands;

public class UpdateTenantCommand : IRequest<ICommandResult<Tenant>>
{
    public UpdateTenantCommand(Guid tenantId, JsonPatchDocument<TenantSm> patch)
    {
        TenantId = tenantId;
        Patch = patch;
    }

    public Guid TenantId { get; protected set; }
    public JsonPatchDocument<TenantSm> Patch { get; protected set; }

    public class UpdateTenantHandler : IRequestHandler<UpdateTenantCommand, ICommandResult<Tenant>>
    {
        public UpdateTenantHandler(ILogger<UpdateTenantHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext,
            IValidator<TenantSm> validator,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<UpdateTenantHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;
        private readonly IValidator<TenantSm> _validator;
        private readonly IMessageBus _messageBus;

        public async Task<ICommandResult<Tenant>> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
        {
            Guid tenantId = command.TenantId;
            JsonPatchDocument<TenantSm> patch = command.Patch;

            if (patch == null)
            {
                return new CommandResult<Tenant>(CommandStatus.BadRequest, null);
            }

            Tenant tenant = await _dbContext.GetTenantWithAllRelativesAsync(tenantId, cancellationToken);

            if (tenant == null)
            {
                return new CommandResult<Tenant>(CommandStatus.NotFound, null);
            }

            TenantSm tenantSm = _mapper.Map<TenantSm>(tenant);

            patch.ApplyTo(tenantSm);

            ValidationResult validationResult = _validator.Validate(tenantSm);

            if (!validationResult.IsValid)
            {
                return new CommandResult<Tenant>(CommandStatus.ModelInvalid, null, validationResult.Errors.Select(e => e.ErrorMessage).ToArray());
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
