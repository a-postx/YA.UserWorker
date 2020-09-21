using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.CommandsAndQueries.Tenants.Commands;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public class PatchTenantByIdAh : IPatchTenantByIdAh
    {
        public PatchTenantByIdAh(ILogger<PatchTenantAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IValidationProblemDetailsGenerator generator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _problemDetailsGenerator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        private readonly ILogger<PatchTenantAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;
        private readonly IValidationProblemDetailsGenerator _problemDetailsGenerator;

        public async Task<IActionResult> ExecuteAsync(Guid yaTenantId, JsonPatchDocument<TenantSm> patch, CancellationToken cancellationToken)
        {
            ICommandResult<TenantVm> result = await _mediator
                .Send(new PatchTenantByIdCommand(yaTenantId, patch), cancellationToken);

            switch (result.Status)
            {
                case CommandStatuses.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatuses.ModelInvalid:
                    ValidationProblemDetails problemDetails = _problemDetailsGenerator.Generate(result.ValidationResult);
                    return new BadRequestObjectResult(problemDetails);
                case CommandStatuses.BadRequest:
                    return new BadRequestResult();
                case CommandStatuses.NotFound:
                    return new NotFoundResult();
                case CommandStatuses.Ok:
                    return new OkObjectResult(result.Data);
            }
        }
    }
}
