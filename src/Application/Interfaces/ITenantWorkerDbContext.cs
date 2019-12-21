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

        Task CreateEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class;
        Task<T> CreateAndReturnEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class;
        Task CreateEntitiesAsync<T>(List<T> newItems, CancellationToken cancellationToken) where T : class;
        Task<T> GetEntityAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
        Task<T> GetEntityWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;
        Task<ICollection<T>> GetEntitiesPagedAsync<T>(Tenant tenant, int page, int count, CancellationToken cancellationToken) where T : class, ITenantEntity, IRowVersionedEntity;
        Task<ICollection<T>> GetEntitiesOrderedAndPagedAsync<T>(Expression<Func<T, Guid>> orderPredicate, int page, int count, CancellationToken cancellationToken) where T : class;
        Task<ICollection<T>> GetEntitiesOrderedAndFilteredAndPagedAsync<T>(Expression<Func<T, byte[]>> orderPredicate, Expression<Func<T, bool>> wherePredicate, int page, int count, CancellationToken cancellationToken) where T : class;

        Task<(int totalCount, int totalPages)> GetTotalPagesAsync<T>(int count, CancellationToken cancellationToken) where T : class;

        Task<List<T>> GetEntitiesPaged<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedReverse<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetEntitiesPagedTask<T>(int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasNextPage<T>(int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetEntitiesPagedHasPreviousPage<T>(int? last, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasNextPage<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetHasPreviousPage<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity;
        Task<int> GetEntitiesCountAsync<T>(CancellationToken cancellationToken) where T : class;

        Task<List<T>> GetTenantEntitiesPaged<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetTenantEntitiesPagedReverse<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<List<T>> GetTenantEntitiesPagedTask<T>(Expression<Func<T, bool>> wherePredicate, int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetTenantEntitiesPagedHasNextPage<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetTenantEntitiesPagedHasPreviousPage<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetTenantHasNextPage<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<bool> GetTenantHasPreviousPage<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity;
        Task<int> GetTenantEntitiesCountAsync<T>(Expression<Func<T, bool>> wherePredicate, CancellationToken cancellationToken) where T : class, ITenantEntity;

        int ApplyChanges();
        Task<int> ApplyChangesAsync(CancellationToken cancellationToken = default);
    }
}
