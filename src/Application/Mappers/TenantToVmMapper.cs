using System;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.ViewModels;
using Delobytes.Mapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YA.TenantWorker.Application.Mappers
{
    public class TenantToVmMapper : IMapper<Tenant, TenantVm>
    {
        public TenantToVmMapper(ILogger<TenantToVmMapper> logger, IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            _log = logger;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        private readonly ILogger<TenantToVmMapper> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

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
            destination.Url = _linkGenerator.GetUriByRouteValues(_httpContextAccessor.HttpContext, RouteNames.GetTenant, new { tenantId });
        }
    }
}
