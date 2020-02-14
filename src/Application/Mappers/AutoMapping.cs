using AutoMapper;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Mappers
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Tenant, TenantSm>();
            CreateMap<TenantSm, Tenant>();
            CreateMap<Tenant, TenantTm>();
            CreateMap<TenantTm, Tenant>();
            CreateMap<PricingTier, PricingTierTm>();
            CreateMap<PricingTierTm, PricingTier>();            
        }
    }
}
