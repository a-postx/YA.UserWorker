using System;
using Microsoft.AspNetCore.Mvc;
using Delobytes.AspNetCore;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.ViewModels;
using Delobytes.Mapper;
using Microsoft.Extensions.Logging;

namespace YA.TenantWorker.Application.Mappers
{
    public class TenantToVmMapper : IMapper<Tenant, TenantVm>
    {
        public TenantToVmMapper(ILogger<TenantToVmMapper> logger, IUrlHelper urlHelper)
        {
            _log = logger;
            _urlHelper = urlHelper;
        }

        private readonly IUrlHelper _urlHelper;
        private readonly ILogger<TenantToVmMapper> _log;

        public void Map(Tenant source, TenantVm destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            Guid tenantId = source.TenantID;

            destination.TenantId = source.TenantID;
            destination.TenantName = source.TenantName;

            //property name of anonymous route value object must correspond to controller http route values
            destination.Url = _urlHelper.AbsoluteRouteUrl(RouteNames.GetTenant, new { tenantId });
            _log.LogInformation("HOST " + _urlHelper.ActionContext.HttpContext.Request.Host.Host);
            _log.LogInformation("HOOOST " + _urlHelper.ActionContext.HttpContext.Request.Host.ToUriComponent());
            _log.LogInformation("Request aborted: " + _urlHelper.ActionContext.HttpContext.RequestAborted.IsCancellationRequested);
        }
    }
}
