using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public class GetUserAh : IGetUserAh
    {
        public GetUserAh(ILogger<GetUserAh> logger,
            IActionContextAccessor actionCtx,
            IRuntimeContextAccessor runtimeContext,
            IMediator mediator,
            IMapper mapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private readonly ILogger<GetUserAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public async Task<IActionResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            (string userProvider, string userExternalId) = _runtimeCtx.GetUserIdentifiers();

            ICommandResult<User> result = await _mediator
                .Send(new GetUserCommand(userProvider, userExternalId), cancellationToken);

            switch (result.Status)
            {
                case CommandStatus.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null);
                case CommandStatus.NotFound:
                    return new NotFoundResult();
                case CommandStatus.Ok:
                    if (_actionCtx.ActionContext.HttpContext
                        .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                    {
                        if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                        {
                            return new StatusCodeResult(StatusCodes.Status304NotModified);
                        }
                    }

                    _actionCtx.ActionContext.HttpContext
                        .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                    UserVm userViewModel = _mapper.Map<UserVm>(result.Data);

                    return new OkObjectResult(userViewModel);
            }
        }
    }
}