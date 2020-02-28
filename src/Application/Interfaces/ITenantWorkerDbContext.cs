using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface ITenantWorkerDbContext
    {
        void DeleteTenant(Tenant tenant);
        void UpdateTenant(Tenant tenant);
        Task<Tenant> GetTenantWithPricingTierAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);

        Task CreateEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class;
        Task<T> CreateAndReturnEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class;
        Task CreateEntitiesAsync<T>(List<T> newItems, CancellationToken cancellationToken) where T : class;
        Task<T> GetEntityAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
        Task<T> GetEntityWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;
        Task<List<T>> GetEntitiesFromAllTenantsWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;

        Task<List<T>> GetEntitiesPagedAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedReverseAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedTaskAsync<T>(int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<int> GetEntitiesCountAsync<T>(CancellationToken cancellationToken) where T : class;

        int ApplyChanges();
        Task<int> ApplyChangesAsync(CancellationToken cancellationToken = default);
    }
}
