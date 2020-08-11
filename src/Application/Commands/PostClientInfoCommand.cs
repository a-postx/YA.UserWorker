using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.Common.Extensions;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Commands
{
    public class PostClientInfoCommand : IPostClientInfoCommand
    {
        public PostClientInfoCommand(ILogger<PostClientInfoCommand> logger,
            IActionContextAccessor actionContextAccessor)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        }

        private readonly ILogger<PostClientInfoCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;

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
