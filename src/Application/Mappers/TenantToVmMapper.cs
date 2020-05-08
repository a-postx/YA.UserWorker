using System;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Models.ViewModels;
using Delobytes.Mapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using AutoMapper;

namespace YA.TenantWorker.Application.Mappers
{
    public class TenantToVmMapper : IMapper<Tenant, TenantVm>
    {
        public TenantToVmMapper(ILogger<TenantToVmMapper> logger,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            IMapper mapper)
        {
            _log = logger;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
        }

        private readonly ILogger<TenantToVmMapper> _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;

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
            destination.PricingTierActivatedUntil = source.PricingTierActivatedUntilDateTime;

            if (source.PricingTier != null)
            {
                destination.PricingTier = _mapper.Map<PricingTierVm>(source.PricingTier);
            }

            RouteData routeData = _httpContextAccessor.HttpContext.GetRouteData();
            string route = (string)routeData.Values["controller"] + (string)routeData.Values["action"];

            if (route.Contains("All") || route.Contains("ById") || route.Contains("Post"))
            {
                //property name of anonymous route value object must correspond to controller http route values
                destination.Url = new Uri(_linkGenerator.GetUriByName(_httpContextAccessor.HttpContext, RouteNames.GetTenantById, new { tenantId }));
            }
            else
            {
                destination.Url = new Uri(_linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext));
            }
            
            ////var hhh = _linkGenerator.GetUriByName(_httpContextAccessor.HttpContext, RouteNames.GetTenant, new { tenantId });
            ////bool gotGwHost = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Gateway-Base-Url", out StringValues baseUrl);
            ////if (gotGwHost)
            ////{
            ////    string[] gwHostValues = baseUrl.ToString().Split(':');
            ////    string gwScheme = gwHostValues[0];
            ////    string gwHost = gwHostValues[1].Replace("//", "");
            ////    int gwPort = int.Parse(gwHostValues[2]);
            ////    _httpContextAccessor.HttpContext.Request.Host = new HostString(gwHost, gwPort);
            ////    destination.Url = _linkGenerator.GetUriByPage(_httpContextAccessor.HttpContext);
            ////}
            ////else
            ////{
            ////    destination.Url = _linkGenerator.GetUriByPage(_httpContextAccessor.HttpContext);
            ////}
        }
    }
}
