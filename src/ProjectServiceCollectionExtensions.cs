using Microsoft.Extensions.DependencyInjection;
using YA.TenantWorker.Commands;
using YA.TenantWorker.Mappers;
using YA.TenantWorker.Models;
using YA.TenantWorker.DAL;
using YA.TenantWorker.Services;
using YA.TenantWorker.ViewModels;
using YA.TenantWorker.SaveModels;
using Delobytes.Mapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

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
                .AddSingleton<IGetTenantCommand, GetTenantCommand>()
                .AddSingleton<IGetTenantPageCommand, GetTenantPageCommand>()
                .AddSingleton<IPostTenantCommand, PostTenantCommand>()
                .AddSingleton<IPatchTenantCommand, PatchTenantCommand>()
                .AddSingleton<IDeleteTenantCommand, DeleteTenantCommand>();
        }

        /// <summary>
        /// Add project mappers to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectMappers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMapper<TenantSm, Tenant>, TenantToSmMapper>()
                .AddSingleton<IMapper<Tenant, TenantSm>, TenantToSmMapper>()
                .AddSingleton<IMapper<Tenant, TenantVm>, TenantToVmMapper>();
        }

        /// <summary>
        /// Add repositories to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<ITenantManagerDbContext, TenantManagerDbContext>();
        }

        /// <summary>
        /// Add internal services to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectServices(this IServiceCollection services, KeyVaultSecrets secrets)
        {
            return services
                .AddSingleton<IClockService, Clock>()
                .AddSingleton<IGeoDataService, IpApiGeoData>();
        }
    }
}
