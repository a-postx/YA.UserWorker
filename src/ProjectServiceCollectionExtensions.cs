using Delobytes.Mapper;
using Microsoft.Extensions.DependencyInjection;
using YA.TenantWorker.Application;
using YA.TenantWorker.Application.Commands;
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
        /// Add available commands to the service collection.
        /// </summary>
        public static IServiceCollection AddProjectCommands(this IServiceCollection services)
        {
            return services
                .AddScoped<IGetTenantCommand, GetTenantCommand>()
                .AddScoped<IGetTenantByIdCommand, GetTenantByIdCommand>()
                .AddScoped<IGetTenantAllPageCommand, GetTenantAllPageCommand>()
                .AddScoped<IPostTenantCommand, PostTenantCommand>()
                .AddScoped<IPatchTenantByIdCommand, PatchTenantByIdCommand>()
                .AddScoped<IPatchTenantCommand, PatchTenantCommand>()
                .AddScoped<IDeleteTenantByIdCommand, DeleteTenantByIdCommand>()
                .AddScoped<IDeleteTenantCommand, DeleteTenantCommand>()

                .AddScoped<IPostClientInfoCommand, PostClientInfoCommand>();
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
                .AddScoped<IRuntimeContextAccessor, RuntimeContextAccessor>()
                .AddSingleton<IRuntimeGeoDataService, IpWhoisRuntimeGeoData>()
                .AddHostedService<MessageBusService>();
        }
    }
}
