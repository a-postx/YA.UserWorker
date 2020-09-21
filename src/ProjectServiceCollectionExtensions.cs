using Delobytes.Mapper;
using Microsoft.Extensions.DependencyInjection;
using YA.TenantWorker.Application;
using YA.TenantWorker.Application.ActionHandlers.ClientInfos;
using YA.TenantWorker.Application.ActionHandlers.Tenants;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Mappers;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Infrastructure.Data;
using YA.TenantWorker.Infrastructure.Messaging;
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
                .AddScoped<IPostTenantAh, PostTenantAh>()
                .AddScoped<IDeleteTenantAh, DeleteTenantAh>()
                .AddScoped<IDeleteTenantByIdAh, DeleteTenantByIdAh>()
                
                .AddScoped<IPostClientInfoAh, PostClientInfoAh>();
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
                .AddSingleton<IMapper<Tenant, TenantVm>, TenantToVmMapper>();
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
                .AddScoped<IMessageBus, MessageBus>()
                .AddScoped<IValidationProblemDetailsGenerator, ValidationProblemDetailsGenerator>()
                .AddScoped<IRuntimeContextAccessor, RuntimeContextAccessor>()
                .AddSingleton<IRuntimeGeoDataService, IpWhoisRuntimeGeoData>()
                .AddHostedService<MessageBusService>();
        }
    }
}
