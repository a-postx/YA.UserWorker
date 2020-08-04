using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YA.Common;
using YA.Common.Extensions;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Infrastructure.Data
{
    public class TenantWorkerDbContext : DbContext, ITenantWorkerDbContext
    {
        public TenantWorkerDbContext(DbContextOptions options, IRuntimeContextAccessor runtimeContext) : base(options)
        {
            if (runtimeContext == null)
            {
                throw new ArgumentNullException(nameof(runtimeContext));
            }

            _tenantId = runtimeContext.GetTenantId();
        }

        public DbSet<ApiRequest> ApiRequests { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<PricingTier> PricingTiers { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly Guid _tenantId;
        private IDbContextTransaction _currentTransaction;

        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;
        private static bool IsMustHaveTenantFilterEnabled => true;
        private static bool IsSoftDeleteFilterEnabled => true;

        private static MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(TenantWorkerDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantWorkerDbContext).Assembly);

            modelBuilder.Seed();

            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureGlobalFiltersMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });
            }
        }

        #region Generics
        public async Task<T> GetEntityWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity
        {
            return await Set<T>().Include(nameof(Tenant)).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<List<T>> GetEntitiesFromAllTenantsWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity
        {
            return await Set<T>().Include(nameof(Tenant)).Where(predicate).ToListAsync(cancellationToken);
        }

        public Task<List<T>> GetEntitiesPagedAsync<T>(int? first, DateTimeOffset? createdAfter,
            DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                .If(first.HasValue, x => x.Take(first.Value))
                .ToList());
        }

        public Task<List<T>> GetEntitiesPagedReverseAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                //converting to Enumerable because of error "This overload of the method 'System.Linq.Queryable.TakeLast' is currently not supported." in v.4.0.3.0
                .If(last.HasValue, x => x.AsEnumerable().TakeLast(last.Value))
                .ToList());
        }

        public Task<bool> GetEntitiesPagedHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .Skip(first.Value)
                .Any());
        }

        public Task<bool> GetEntitiesPagedHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().OrderBy(t => t.tstamp)
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                //converting to Enumerable because of error "This overload of the method 'System.Linq.Queryable.SkipLast' is currently not supported." in v.4.0.3.0
                .AsEnumerable().SkipLast(last.Value)
                .Any());
        }

        public Task<List<T>> GetEntitiesPagedTaskAsync<T>(int? first, int? last, DateTimeOffset? createdAfter,
            DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            Task<List<T>> getTenantsTask;

            if (first.HasValue)
            {
                getTenantsTask = GetEntitiesPagedAsync<T>(first, createdAfter, createdBefore, cancellationToken);
            }
            else
            {
                getTenantsTask = GetEntitiesPagedReverseAsync<T>(last, createdAfter, createdBefore, cancellationToken);
            }

            return getTenantsTask;
        }

        public async Task<bool> GetHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter,
            DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            if (first.HasValue)
            {
                return await GetEntitiesPagedHasNextPageAsync<T>(first, createdAfter, cancellationToken);
            }
            else if (createdBefore.HasValue)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> GetHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdAfter,
            DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
        {
            if (last.HasValue)
            {
                return await GetEntitiesPagedHasPreviousPageAsync<T>(last, createdBefore, cancellationToken);
            }
            else if (createdAfter.HasValue)
            {
                return true;
            }

            return false;
        }

        public async Task<int> GetEntitiesCountAsync<T>(CancellationToken cancellationToken) where T : class
        {
            return await Set<T>().CountAsync(cancellationToken);
        }
        #endregion

        #region ApiRequests
        public async Task<ApiRequest> CreateApiRequestAsync(ApiRequest item, CancellationToken cancellationToken)
        {
            EntityEntry<ApiRequest> entityEntry = await Set<ApiRequest>().AddAsync(item, cancellationToken);
            return entityEntry.Entity;
        }

        public async Task<ApiRequest> GetApiRequestAsync(Expression<Func<ApiRequest, bool>> predicate, CancellationToken cancellationToken)
        {
            return await Set<ApiRequest>().SingleOrDefaultAsync(predicate, cancellationToken);
        }
        #endregion

        #region Tenants
        public async Task CreateTenantAsync(Tenant item, CancellationToken cancellationToken)
        {
            await Set<Tenant>().AddAsync(item, cancellationToken);
        }

        public async Task<Tenant> GetTenantAsync(CancellationToken cancellationToken)
        {
            return await Tenants.SingleOrDefaultAsync(e => e.TenantID == _tenantId, cancellationToken);
        }

        public async Task<Tenant> GetTenantAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
        {
            return await Set<Tenant>().SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<Tenant> GetTenantWithPricingTierAsync(CancellationToken cancellationToken)
        {
            return await Set<Tenant>().Include(nameof(PricingTier)).SingleOrDefaultAsync(e => e.TenantID == _tenantId, cancellationToken);
        }

        public void UpdateTenant(Tenant item)
        {
            Set<Tenant>().Update(item);
        }

        public void DeleteTenant(Tenant item)
        {
            Set<Tenant>().Remove(item);
        }
        #endregion

        #region Filtering
        private void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType) where TEntity : class
        {
            if (entityType.BaseType == null && ShouldFilterEntity<TEntity>(entityType))
            {
                Expression<Func<TEntity, bool>> filterExpression = CreateFilterExpression<TEntity>();

                if (filterExpression != null)
                {
                    if (entityType.IsKeyless)
                    {
                        modelBuilder.Entity<TEntity>().HasNoKey().HasQueryFilter(filterExpression);
                    }
                    else
                    {
                        modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
                    }
                }
            }
        }

        private bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
        {
            if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }

            if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }

            return false;
        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>() where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> mustHaveTenantFilter = e => !IsMustHaveTenantFilterEnabled || ((ITenantEntity)e).Tenant.TenantID == _tenantId;
                expression = expression == null ? mustHaveTenantFilter : CombineExpressions(expression, mustHaveTenantFilter);
            }

            if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> softDeleteFilter = e => !IsSoftDeleteFilterEnabled || !((ISoftDeleteEntity)e).IsDeleted;
                expression = expression == null ? softDeleteFilter : CombineExpressions(expression, softDeleteFilter);
            }

            return expression;
        }

        private Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            return ExpressionCombiner.Combine(expression1, expression2);
        }
        #endregion

        #region Saving
        private void ApplySavingConcepts()
        {
            foreach (EntityEntry entry in ChangeTracker.Entries().ToList())
            {
                if (entry.State != EntityState.Modified && entry.CheckOwnedEntityChange())
                {
                    Entry(entry.Entity).State = EntityState.Modified;
                }

                ApplySavingConcepts(entry);
            }
        }

        private void ApplySavingConcepts(EntityEntry entry)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyConceptsForAddedEntity(entry);
                    break;
                case EntityState.Modified:
                    ApplyConceptsForModifiedEntity(entry);
                    break;
                case EntityState.Deleted:
                    ApplyConceptsForDeletedEntity(entry);
                    break;
            }
        }

        private void ApplyConceptsForAddedEntity(EntityEntry entry)
        {
            SetCreationAuditProperties(entry.Entity);
        }

        private void ApplyConceptsForModifiedEntity(EntityEntry entry)
        {
            SetModificationAuditProperties(entry.Entity);
        }

        private void ApplyConceptsForDeletedEntity(EntityEntry entry)
        {
            CancelDeletionForSoftDelete(entry);
            SetDeletionAuditProperties(entry.Entity);
        }

        private static void SetCreationAuditProperties(object entityAsObj)
        {
            if (entityAsObj is IAuditedEntityBase)
            {
                IAuditedEntityBase auditedEntity = entityAsObj.As<IAuditedEntityBase>();
                auditedEntity.LastModifiedDateTime = DateTime.UtcNow;
            }
        }

        private static void SetModificationAuditProperties(object entityAsObj)
        {
            if (entityAsObj is IAuditedEntityBase)
            {
                entityAsObj.As<IAuditedEntityBase>().LastModifiedDateTime = DateTime.UtcNow;
            }
        }

        private static void SetDeletionAuditProperties(object entityAsObj)
        {
            if (entityAsObj is IAuditedEntityBase)
            {
                // модифицируем для мягко удалённых сущностей
                entityAsObj.As<IAuditedEntityBase>().LastModifiedDateTime = DateTime.UtcNow;
            }
        }

        private void CancelDeletionForSoftDelete(EntityEntry entry)
        {
            if (!(entry.Entity is ISoftDeleteEntity))
            {
                return;
            }

            entry.Reload();
            entry.State = EntityState.Modified;
            entry.Entity.As<ISoftDeleteEntity>().IsDeleted = true;
        }

        private void MakeSureSaveWithSingleTenantId()
        {
            IEnumerable<EntityEntry<ITenantEntity>> tenantEntities = ChangeTracker.Entries<ITenantEntity>();

            if (tenantEntities.Any())
            {
                int distinctTenantIdsCount = tenantEntities.Select(e => e.Entity.Tenant?.TenantID)
                                     .Distinct().Count();

                if (distinctTenantIdsCount > 1)
                {
                    throw new Exception("More than one TenantID detected.");
                }
            }
        }

        public int ApplyChanges()
        {
            ApplySavingConcepts();
            MakeSureSaveWithSingleTenantId();
            return base.SaveChanges();
        }

        public async Task<int> ApplyChangesAsync(CancellationToken cancellationToken)
        {
            ApplySavingConcepts();
            MakeSureSaveWithSingleTenantId();
            return await base.SaveChangesAsync(cancellationToken);
        }
        #endregion

        internal async Task RunQueryAsync(string sqlQuery)
        {
            using (DbQueryRunner runner = new DbQueryRunner(this))
            {
                await runner.RunQueryAsync(sqlQuery);
            }
        }

        #region Transactions
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null;

            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        #endregion
    }
}