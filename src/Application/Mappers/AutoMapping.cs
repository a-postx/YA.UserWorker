using AutoMapper;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Mappers
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<User, UserVm>().ReverseMap();
            CreateMap<User, UserSm>().ReverseMap();

            CreateMap<UserSetting, UserSettingVm>().ReverseMap();
            CreateMap<UserSetting, UserSettingSm>().ReverseMap();

            CreateMap<Tenant, TenantSm>().ReverseMap();
            CreateMap<Tenant, TenantTm>().ReverseMap();
            CreateMap<Tenant, TenantVm>().ReverseMap();

            CreateMap<PricingTier, PricingTierTm>().ReverseMap();
            CreateMap<PricingTier, PricingTierVm>().ReverseMap();

            CreateMap<ClientInfoSm, ClientInfoTm>();
            CreateMap<ClientInfoTm, YaClientInfo>();

            CreateMap<Membership, MembershipVm>().ReverseMap();
        }
    }
}
