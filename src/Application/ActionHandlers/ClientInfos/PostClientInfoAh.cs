using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using YA.Common.Constants;
using YA.UserWorker.Application.Features.ClientInfos.Commands;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;

namespace YA.UserWorker.Application.ActionHandlers.ClientInfos;

public class PostClientInfoAh : IPostClientInfoAh
{
    public PostClientInfoAh(ILogger<PostClientInfoAh> logger,
        IHttpContextAccessor httpCtx,
        IMediator mediator,
        IMapper mapper)
    {
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpCtx = httpCtx ?? throw new ArgumentNullException(nameof(httpCtx));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private readonly ILogger<PostClientInfoAh> _log;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public async Task<IActionResult> ExecuteAsync(ClientInfoSm clientInfoSm, CancellationToken cancellationToken)
    {
        if (clientInfoSm == null)
        {
            return new BadRequestResult();
        }

        await Task.Delay(10, cancellationToken);

        string clientIp = "unknown";

        if (_httpCtx.HttpContext.Request.Headers.TryGetValue("X-Original-For", out StringValues forwardedValue))
        {
            string clientIpsList = forwardedValue.ToString();
            string clientIpElement = clientIpsList.Split(',').Select(s => s.Trim()).FirstOrDefault();

            if (!string.IsNullOrEmpty(clientIpElement))
            {
                string ip = clientIpElement.Split(':').Select(s => s.Trim()).FirstOrDefault();

                if (!string.IsNullOrEmpty(ip))
                {
                    clientIp = ip;
                }
            }
        }
        else
        {
            string ip = _httpCtx.HttpContext.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ip))
            {
                clientIp = ip;
            }
        }

        string username = "unknown";

        string usernameClaim = _httpCtx.HttpContext.User.Claims.Where(c => c.Type == YaClaimNames.name).FirstOrDefault()?.Value;

        if (!string.IsNullOrEmpty(usernameClaim))
        {
            username = usernameClaim;
        }

        using (_log.BeginScopeWith(("ClientVersion", clientInfoSm.ClientVersion), ("Browser", clientInfoSm.Browser),
                ("BrowserVersion", clientInfoSm.BrowserVersion), ("Os", clientInfoSm.Os),
                ("OsVersion", clientInfoSm.OsVersion), ("DeviceModel", clientInfoSm.DeviceModel),
                ("CountryName", clientInfoSm.CountryName), ("RegionName", clientInfoSm.RegionName),
                ("ScreenResolution", clientInfoSm.ScreenResolution), ("ViewportSize", clientInfoSm.ViewportSize),
                ("Timestamp", clientInfoSm.Timestamp)))
        {
            _log.LogInformation("ClientInfo");
        }

        ClientInfoTm clientInfoTm = _mapper.Map<ClientInfoTm>(clientInfoSm);
        clientInfoTm.Username = username;
        clientInfoTm.IpAddress = clientIp;

        ICommandResult result = await _mediator
            .Send(new CreateClientInfoCommand(clientInfoTm), cancellationToken);

        ClientInfoVm clientInfoVm = new ClientInfoVm(true);

        return new OkObjectResult(clientInfoVm);
    }
}
