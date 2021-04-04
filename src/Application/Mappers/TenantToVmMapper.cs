using System;
using System.Collections.Generic;
using AutoMapper;
using Delobytes.Mapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Mappers
{
    /// <summary>
    /// Ручной мапер сущностей, от автомапных отличается тем, что может проставлять УРЛ.
    /// </summary>
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
            destination.Name = source.Name;
            destination.PricingTierId = source.PricingTierId;
            destination.PricingTierActivatedDateTime = source.PricingTierActivatedDateTime;
            destination.PricingTierActivatedUntilDateTime = source.PricingTierActivatedUntilDateTime;

            if (source.PricingTier != null)
            {
                destination.PricingTier = _mapper.Map<PricingTierVm>(source.PricingTier);
            }

            if (source.Memberships?.Count > 0)
            {
                destination.Memberships = new List<MembershipVm>();

                foreach (Membership item in source.Memberships)
                {
                    MembershipVm membership = _mapper.Map<MembershipVm>(item);
                    destination.Memberships.Add(membership);
                }
            }

            RouteData routeData = _httpContextAccessor.HttpContext.GetRouteData();
            string route = (string)routeData.Values["controller"] + (string)routeData.Values["action"];

            if (route.Contains("All", StringComparison.Ordinal) || route.Contains("ById", StringComparison.Ordinal))
            {
                //property name of anonymous route value object must correspond to controller http route values
                destination.Url = new Uri(_linkGenerator.GetUriByName(_httpContextAccessor.HttpContext, RouteNames.GetTenantById, new { tenantId }));
            }
            else if (route.Contains("Post", StringComparison.Ordinal))
            {
                destination.Url = new Uri(_linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext));
            }
            else
            {
                destination.Url = new Uri(_linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext));
            }
        }
    }
}
