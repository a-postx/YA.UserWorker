using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Extensions;

namespace YA.TenantWorker.Application.ActionHandlers.ClientInfos
{
    public class PostClientInfoAh : IPostClientInfoAh
    {
        public PostClientInfoAh(ILogger<PostClientInfoAh> logger,
            IActionContextAccessor actionCtx,
            IMediator mediator)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private readonly ILogger<PostClientInfoAh> _log;
        private readonly IActionContextAccessor _actionCtx;
        private readonly IMediator _mediator;

        public async Task<IActionResult> ExecuteAsync(ClientInfoSm clientInfoSm, CancellationToken cancellationToken)
        {
            if (clientInfoSm == null)
            {
                return new BadRequestResult();
            }

            await Task.Delay(10, cancellationToken);

            using (_log.BeginScopeWith(("Browser", clientInfoSm.Browser), ("BrowserVersion", clientInfoSm.BrowserVersion),
                    ("Os", clientInfoSm.Os), ("OsVersion", clientInfoSm.OsVersion),
                    ("DeviceModel", clientInfoSm.DeviceModel),
                    ("CountryName", clientInfoSm.CountryName), ("RegionName", clientInfoSm.RegionName),
                    ("ScreenResolution", clientInfoSm.ScreenResolution), ("ViewportSize", clientInfoSm.ViewportSize),
                    ("Timestamp", clientInfoSm.Timestamp)))
            {
                _log.LogInformation("ClientInfo");
            }

            ClientInfoVm clientInfoVm = new ClientInfoVm { Success = true };

            return new OkObjectResult(clientInfoVm);
        }
    }
}
