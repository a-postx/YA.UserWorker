using Delobytes.Mapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using YA.UserWorker.Application.ActionHandlers.ClientInfos;
using YA.UserWorker.Application.ActionHandlers.Invitations;
using YA.UserWorker.Application.ActionHandlers.Memberships;
using YA.UserWorker.Application.ActionHandlers.Tenants;
using YA.UserWorker.Application.ActionHandlers.Users;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Mappers;
using YA.UserWorker.Application.Models.ViewModels;
using YA.UserWorker.Application.Services;
using YA.UserWorker.Core.Entities;
using YA.UserWorker.Infrastructure.Authentication;
using YA.UserWorker.Infrastructure.Data;
using YA.UserWorker.Infrastructure.Messaging;
using YA.UserWorker.Infrastructure.Services;

namespace YA.UserWorker.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods add project services.
    /// </summary>
    /// <remarks>
    /// AddSingleton - Only one instance is ever created and returned.
    /// AddScoped - A new instance is created and returned for each request/response cycle.
    /// AddTransient - A new instance is created and returned each time.
    /// </remarks>
    public static class ProjectServiceCollectionExtensions
    {
        /// <summary>
        /// Add available action handlers to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectActionHandlers(this IServiceCollection services)
        {
            return services
                .AddScoped<IGetTenantAh, GetTenantAh>()
                .AddScoped<IGetTenantByIdAh, GetTenantByIdAh>()
                .AddScoped<IGetTenantAllPageAh, GetTenantAllPageAh>()
                .AddScoped<IPatchTenantAh, PatchTenantAh>()
                .AddScoped<IPatchTenantByIdAh, PatchTenantByIdAh>()
                .AddScoped<IDeleteTenantAh, DeleteTenantAh>()
                .AddScoped<IDeleteTenantByIdAh, DeleteTenantByIdAh>()

                .AddScoped<IGetUserAh, GetUserAh>()
                .AddScoped<IPatchUserAh, PatchUserAh>()
                .AddScoped<IRegisterNewUserAh, RegisterNewUserAh>()
                .AddScoped<ISwitchUserTenantAh, SwitchUserTenantAh>()

                .AddScoped<IPostInvitationAh, PostInvitationAh>()
                .AddScoped<IGetInvitationAh, GetInvitationAh>()
                .AddScoped<IDeleteInvitationAh, DeleteInvitationAh>()

                .AddScoped<IPostMembershipAh, PostMembershipAh>()
                .AddScoped<IDeleteMembershipAh, DeleteMembershipAh>()

                .AddScoped<IPostClientInfoAh, PostClientInfoAh>();
        }

        /// <summary>
        /// Add project app components to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectComponents(this IServiceCollection services)
        {
            return services
                .AddScoped<IPaginatedResultFactory, PaginatedResultFactory>();
        }

        /// <summary>
        /// Add project mappers to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectMappers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMapper<Tenant, TenantVm>, TenantToVmMapper>();
        }

        /// <summary>
        /// Add repositories to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<IUserWorkerDbContext, UserWorkerDbContext>();
                //.AddScoped<IRootDbContext, RootDbContext>();
        }

        /// <summary>
        /// Add internal services to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IClockService, Clock>()
                .AddScoped<IMessageBus, MessageBus>()
                .AddScoped<IRuntimeContextAccessor, RuntimeContextAccessor>()
                .AddSingleton<IRuntimeGeoDataService, IpWhoisRuntimeGeoData>()
                .AddScoped<IAuthProviderManager, Auth0AuthProviderManager>();
        }

        /// <summary>
        /// Добавляет кастомизированную фабрику Деталей Проблемы.
        /// </summary>
        public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
        {
            services
                .AddTransient<IProblemDetailsFactory, YaProblemDetailsFactory>()
                .AddTransient<ProblemDetailsFactory, YaProblemDetailsFactory>();

            return services;
        }
    }
}
