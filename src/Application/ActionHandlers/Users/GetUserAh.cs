using System.Globalization;
using AutoMapper;
using Delobytes.AspNetCore.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using YA.UserWorker.Application.Features.Users.Queries;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.ActionHandlers.Users;

public class GetUserAh : IGetUserAh
{
    public GetUserAh(ILogger<GetUserAh> logger,
        IHttpContextAccessor httpCtx,
        IRuntimeContextAccessor runtimeContext,
        IMediator mediator,
        IMapper mapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _runtimeCtx = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private readonly ILogger<GetUserAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
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
                if (_httpCtx.HttpContext
                    .Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out StringValues stringValues))
                {
                    if (DateTimeOffset.TryParse(stringValues, out DateTimeOffset modifiedSince) && (modifiedSince >= result.Data.LastModifiedDateTime))
                    {
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                }

                _httpCtx.HttpContext
                    .Response.Headers.Add(HeaderNames.LastModified, result.Data.LastModifiedDateTime.ToString("R", CultureInfo.InvariantCulture));

                UserVm userViewModel = _mapper.Map<UserVm>(result.Data);

                return new OkObjectResult(userViewModel);
        }
    }
}
