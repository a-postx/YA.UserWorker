using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YA.Common.Constants;
using YA.TenantWorker.Application.Features;
using YA.TenantWorker.Application.Features.ClientInfos.Commands;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Extensions;

namespace YA.TenantWorker.Application.ActionHandlers.ClientInfos
{
    public class PostClientInfoAh : IPostClientInfoAh
    {
        public PostClientInfoAh(ILogger<PostClientInfoAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator,
            IMapper mapper)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private readonly ILogger<PostClientInfoAh> _log;
        private readonly IActionContextAccessor _actionCtx;
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

            if (_actionCtx.ActionContext.HttpContext.Request.Headers.TryGetValue("X-Original-For", out StringValues forwardedValue))
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
                string ip = _actionCtx.ActionContext.HttpContext.Connection.RemoteIpAddress?.ToString();

                if (!string.IsNullOrEmpty(ip))
                {
                    clientIp = ip;
                }
            }

            string username = "unknown";

            string usernameClaim = _actionCtx.ActionContext.HttpContext.User.Claims.Where(c => c.Type == YaClaimNames.name).FirstOrDefault()?.Value;

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

            ICommandResult<EmptyCommandResult> result = await _mediator
                .Send(new CreateClientInfoCommand(clientInfoTm), cancellationToken);

            ClientInfoVm clientInfoVm = new ClientInfoVm(true);

            return new OkObjectResult(clientInfoVm);
        }
    }
}
