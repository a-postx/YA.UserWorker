using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features.Users.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public class PatchUserAh : IPatchUserAh
    {
        public PatchUserAh(ILogger<PatchUserAh> logger,
            IActionContextAccessor actionCtx,
            IRuntimeContextAccessor runtimeContext,
            IMediator mediator,
            IProblemDetailsFactory problemDetailsFactory,
            IMapper mapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _pdFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private readonly ILogger<PatchUserAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IMediator _mediator;
        private readonly IProblemDetailsFactory _pdFactory;
        private readonly IMapper _mapper;

        public async Task<IActionResult> ExecuteAsync(JsonPatchDocument<UserSm> patch, CancellationToken cancellationToken)
        {
            (string authProvider, string userExternalId) = _runtimeCtx.GetUserIdentifiers();

            ICommandResult<User> result = await _mediator
                .Send(new UpdateUserCommand(authProvider, userExternalId, patch), cancellationToken);

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
                    UserVm userVm = _mapper.Map<UserVm>(result.Data);
                    return new OkObjectResult(userVm);
            }
        }
    }
}
