using AutoMapper;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Mappers
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Tenant, TenantSm>().ReverseMap();
            CreateMap<Tenant, TenantTm>().ReverseMap();

            CreateMap<PricingTier, PricingTierTm>().ReverseMap();
            CreateMap<PricingTier, PricingTierVm>().ReverseMap();

            CreateMap<ClientInfoSm, ClientInfoTm>();
            CreateMap<ClientInfoTm, YaClientInfo>();
        }
    }
}
