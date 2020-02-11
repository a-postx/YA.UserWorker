using Delobytes.Mapper;
using Microsoft.Extensions.DependencyInjection;
using YA.TenantWorker.Application;
using YA.TenantWorker.Application.Commands;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Mappers;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Infrastructure.Data;
using YA.TenantWorker.Infrastructure.Services;

namespace YA.TenantWorker
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
        /// Add available commands to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectCommands(this IServiceCollection services)
        {
            return services
                .AddScoped<IAuthenticateCommand, AuthenticateCommand>()
                .AddScoped<IGetTenantCommand, GetTenantCommand>()
                .AddScoped<IGetTenantPageCommand, GetTenantPageCommand>()
                .AddScoped<IPostTenantCommand, PostTenantCommand>()
                .AddScoped<IPatchTenantCommand, PatchTenantCommand>()
                .AddScoped<IDeleteTenantCommand, DeleteTenantCommand>();
        }

        /// <summary>
        /// Add project domain components to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectComponents(this IServiceCollection services)
        {
            return services
                .AddScoped<ITenantManager, TenantManager>();
        }

        /// <summary>
        /// Add project mappers to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectMappers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMapper<TenantSm, Tenant>, TenantToSmMapper>()
                .AddSingleton<IMapper<Tenant, TenantSm>, TenantToSmMapper>()
                .AddSingleton<IMapper<Tenant, TenantVm>, TenantToVmMapper>()
                .AddSingleton<IMapper<PricingTier, PricingTierTm>, PricingTierToTmMapper>();
        }

        /// <summary>
        /// Add repositories to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<ITenantWorkerDbContext, TenantWorkerDbContext>();
        }

        /// <summary>
        /// Add internal services to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IClockService, Clock>()
                .AddSingleton<IGeoDataService, IpApiGeoData>()
                .AddHostedService<MessageBusService>();
        }
    }
}
