using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Application.Features.Tenants.Commands;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;
using Delobytes.Mapper;

namespace YA.UserWorker.Application.ActionHandlers.Tenants
{
    public class PatchTenantByIdAh : IPatchTenantByIdAh
    {
        public PatchTenantByIdAh(ILogger<PatchTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IProblemDetailsFactory problemDetailsFactory,
            IMapper<Tenant, TenantVm> tenantVmMapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _pdFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
            _tenantVmMapper = tenantVmMapper ?? throw new ArgumentNullException(nameof(tenantVmMapper));
        }

        private readonly ILogger<PatchTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IProblemDetailsFactory _pdFactory;
        private readonly IMapper<Tenant, TenantVm> _tenantVmMapper;

        public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
        {
            ICommandResult<Tenant> result = await _mediator
                .Send(new UpdateTenantByIdCommand(yaTenantId, patch), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.ModelInvalid:
                    ValidationProblemDetails problemDetails = _pdFactory
                        .CreateValidationProblemDetails(_actionCtx.ActionContext.HttpContext, result.ValidationResult);
                    return new BadRequestObjectResult(problemDetails);
                case CommandStatus.BadRequest:
                    return new BadRequestResult();
                case CommandStatus.NotFound:
                    return new NotFoundResult();
                case CommandStatus.Ok:
                    TenantVm tenantVm = _tenantVmMapper.Map(result.Data);
                    return new OkObjectResult(tenantVm);
            }
        }
    }
}
