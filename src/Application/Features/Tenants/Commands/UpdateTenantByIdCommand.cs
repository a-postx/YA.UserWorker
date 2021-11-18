using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Validators;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Tenants.Commands;

public class UpdateTenantByIdCommand : IRequest<ICommandResult<Tenant>>
{
    public UpdateTenantByIdCommand(Guid tenantId, JsonPatchDocument<TenantSm> patch)
    {
        TenantId = tenantId;
        Patch = patch;
    }

    public Guid TenantId { get; protected set; }
    public JsonPatchDocument<TenantSm> Patch { get; protected set; }

    public class UpdateTenantByIdHandler : IRequestHandler<UpdateTenantByIdCommand, ICommandResult<Tenant>>
    {
        public UpdateTenantByIdHandler(ILogger<UpdateTenantByIdHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext,
            IMessageBus messageBus)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        private readonly ILogger<UpdateTenantByIdHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public async Task<ICommandResult<Tenant>> Handle(UpdateTenantByIdCommand command, CancellationToken cancellationToken)
        {
            Guid tenantId = command.TenantId;
            JsonPatchDocument<TenantSm> patch = command.Patch;

            if (tenantId == Guid.Empty || patch == null)
            {
                return new CommandResult<Tenant>(CommandStatus.BadRequest, null);
            }

            Tenant tenant = await _dbContext.GetTenantAsync(e => e.TenantID == tenantId, cancellationToken);

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
