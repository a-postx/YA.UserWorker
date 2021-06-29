using System;
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
            CreateMap<DateTime, DateTime>().ConvertUsing((s, d) => {
                if (s.Kind == DateTimeKind.Local)
                {
                    d = s.ToUniversalTime();
                    return d;
                }
                else if (s.Kind == DateTimeKind.Utc)
                {
                    d = s;
                    return d;
                }
                else
                {
                    throw new AutoMapperMappingException($"Cannot map DateTime with conversion: unspecified kind.");
                }
            });

            CreateMap<User, UserVm>().ReverseMap();
            CreateMap<User, MembershipUserVm>().ReverseMap();
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

            CreateMap<YaInvitation, InvitationVm>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));
            CreateMap<YaInvitation, InvitationSm>().ReverseMap();
            CreateMap<YaInvitation, InvitationTm>().ReverseMap();
        }
    }
}
