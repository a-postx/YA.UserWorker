using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace YA.TenantWorker.Core.Entities
{
    public interface ITenantWorkerDbContext
    {
        Task<T> GetEntityWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;
        Task<List<T>> GetEntitiesFromAllTenantsWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;
        Task<List<T>> GetEntitiesPagedAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedReverseAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, bool orderDesc, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedTaskAsync<T>(int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdBefore, bool orderDesc, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<int> GetEntitiesCountAsync<T>(CancellationToken cancellationToken) where T : class;

        Task<ApiRequest> CreateApiRequestAsync(ApiRequest item, CancellationToken cancellationToken);
        Task<ApiRequest> GetApiRequestAsync(Expression<Func<ApiRequest, bool>> predicate, CancellationToken cancellationToken);

        Task CreateTenantAsync(Tenant item, CancellationToken cancellationToken);
        Task<Tenant> GetTenantAsync(CancellationToken cancellationToken);
        Task<Tenant> GetTenantAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);
        Task<Tenant> GetTenantWithPricingTierAsync(CancellationToken cancellationToken);
        void UpdateTenant(Tenant item);
        void DeleteTenant(Tenant item);

        int ApplyChanges();
        Task<int> ApplyChangesAsync(CancellationToken cancellationToken);
    }
}
