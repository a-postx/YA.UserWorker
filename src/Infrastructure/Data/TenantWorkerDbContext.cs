using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using YA.TenantWorker.Infrastructure.Data.Configurations;
using YA.TenantWorker.Core.Entities;
using YA.TenantWorker.Application.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YA.TenantWorker.Infrastructure.Data
{
    public class TenantWorkerDbContext : DbContext, ITenantWorkerDbContext
    {
        public TenantWorkerDbContext(DbContextOptions options) : base (options)
        {
             
        }

        public DbSet<ApiRequest> ApiRequests { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<PricingTier> PricingTiers { get; set; }
        public DbSet<User> Users { get; set; }

        private IDbContextTransaction _currentTransaction;

        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ApiRequestConfiguration());
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PricingTierConfiguration());

            modelBuilder.Seed();
        }

        private void DeleteItem<T>(T item) where T : class
        {
            Set<T>().Remove(item);
        }

        private void UpdateItem<T>(T item) where T : class
        {
            Set<T>().Update(item);
        }

        public async Task CreateEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class
        {
            await Set<T>().AddAsync(item, cancellationToken);
        }

        public async Task<T> CreateAndReturnEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class
        {
            EntityEntry<T> entityEntry = await Set<T>().AddAsync(item, cancellationToken);
            return entityEntry.Entity;
        }

        public async Task CreateEntitiesAsync<T>(List<T> newItems, CancellationToken cancellationToken) where T : class
        {
            await Set<T>().AddRangeAsync(newItems, cancellationToken);
        }

        public async Task<T> GetEntityAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return await Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T> GetEntityWithTenantAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity
        {
            return await Set<T>().Include(nameof(Tenant)).SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<ICollection<T>> GetEntitiesPagedAsync<T>(Tenant tenant, int page, int count, CancellationToken cancellationToken) where T : class, ITenantEntity, IRowVersionedEntity
        {
            List<T> items = await Set<T>().OrderBy(t => t.tstamp).Where(t => t.Tenant == tenant)
                .Skip(count * (page - 1)).Take(count).ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                items = null;
            }

            return items;
        }

        public async Task<ICollection<T>> GetEntitiesOrderedAndPagedAsync<T>(Expression<Func<T, Guid>> orderPredicate,
            int page, int count, CancellationToken cancellationToken) where T : class
        {
            List<T> items = await Set<T>().OrderBy(orderPredicate)
                .Skip(count * (page - 1)).Take(count).ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                items = null;
            }

            return items;
        }

        public async Task<ICollection<T>> GetEntitiesOrderedAndFilteredAndPagedAsync<T>(Expression<Func<T, byte[]>> orderPredicate,
            Expression<Func<T, bool>> wherePredicate,
            int page, int count, CancellationToken cancellationToken) where T : class
        {
            List<T> items = await Set<T>().OrderBy(orderPredicate).Where(wherePredicate)
                .Skip(count * (page - 1)).Take(count).ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                items = null;
            }

            return items;
        }

        public async Task<(int totalCount, int totalPages)> GetTotalPagesAsync<T>(int count, CancellationToken cancellationToken) where T : class
        {
            int totalCount = await Set<T>().CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalCount / (double)count);
            return (totalCount, totalPages);
        }

        public Task<List<T>> GetEntitiesPagedAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
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

        public Task<List<T>> GetEntitiesPagedTaskAsync<T>(int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
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

        public async Task<bool> GetHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
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

        public async Task<bool> GetHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
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

        public Task<List<T>> GetTenantEntitiesPagedAsync<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().Where(wherePredicate).OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                .If(first.HasValue, x => x.Take(first.Value))
                .ToList());
        }

        public Task<List<T>> GetTenantEntitiesPagedReverseAsync<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().Where(wherePredicate).OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                //converting to Enumerable because of error "This overload of the method 'System.Linq.Queryable.TakeLast' is currently not supported." in v.4.0.3.0
                .If(last.HasValue, x => x.AsEnumerable().TakeLast(last.Value))
                .ToList());
        }

        public Task<bool> GetTenantEntitiesPagedHasNextPageAsync<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().Where(wherePredicate).OrderBy(t => t.tstamp)
                .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
                .Skip(first.Value)
                .Any());
        }

        public Task<bool> GetTenantEntitiesPagedHasPreviousPageAsync<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            return Task.FromResult(Set<T>().Where(wherePredicate).OrderBy(t => t.tstamp)
                .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
                //converting to Enumerable because of error "This overload of the method 'System.Linq.Queryable.SkipLast' is currently not supported." in v.4.0.3.0
                .AsEnumerable().SkipLast(last.Value)
                .Any());
        }

        public Task<List<T>> GetTenantEntitiesPagedTaskAsync<T>(Expression<Func<T, bool>> wherePredicate, int? first, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            Task<List<T>> getTenantsTask;

            if (first.HasValue)
            {
                getTenantsTask = GetTenantEntitiesPagedAsync<T>(wherePredicate, first, createdAfter, createdBefore, cancellationToken);
            }
            else
            {
                getTenantsTask = GetTenantEntitiesPagedReverseAsync<T>(wherePredicate, last, createdAfter, createdBefore, cancellationToken);
            }

            return getTenantsTask;
        }

        public async Task<bool> GetTenantHasNextPageAsync<T>(Expression<Func<T, bool>> wherePredicate, int? first, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            if (first.HasValue)
            {
                return await GetTenantEntitiesPagedHasNextPageAsync<T>(wherePredicate, first, createdAfter, cancellationToken);
            }
            else if (createdBefore.HasValue)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> GetTenantHasPreviousPageAsync<T>(Expression<Func<T, bool>> wherePredicate, int? last, DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, CancellationToken cancellationToken) where T : class, ITenantEntity, IAuditedEntityBase, IRowVersionedEntity
        {
            if (last.HasValue)
            {
                return await GetTenantEntitiesPagedHasPreviousPageAsync<T>(wherePredicate, last, createdBefore, cancellationToken);
            }
            else if (createdAfter.HasValue)
            {
                return true;
            }

            return false;
        }

        public async Task<int> GetTenantEntitiesCountAsync<T>(Expression<Func<T, bool>> wherePredicate, CancellationToken cancellationToken) where T : class, ITenantEntity
        {
            return await Set<T>().Where(wherePredicate).CountAsync(cancellationToken);
        }

        #region Tenants
        public void DeleteTenant(Tenant tenant)
        {
            DeleteItem(tenant);
        }

        public void UpdateTenant(Tenant tenant)
        {
            UpdateItem(tenant);
        }
        #endregion

        private void ApplyAuditedValues()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            List<EntityEntry> entries = ChangeTracker.Entries().ToList();

            List<EntityEntry> updatedEntries = entries.Where(e => e.Entity is IAuditedEntityBase && e.State == EntityState.Modified).ToList();

            updatedEntries.ForEach(e =>
            {
                ((IAuditedEntityBase)e.Entity).LastModifiedDateTime = currentDateTime;
            });
        }

        public int ApplyChanges()
        {
            ApplyAuditedValues();

            return base.SaveChanges();
        }

        public async Task<int> ApplyChangesAsync(CancellationToken cancellationToken)
        {
            ApplyAuditedValues();

            int result = 0;

            bool saved = false;

            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    result = await base.SaveChangesAsync(cancellationToken);
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Tenant)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                // TODO: decide which value should be written to database
                                proposedValues[property] = proposedValue;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException("Don't know how to handle concurrency conflicts for " + entry.Metadata.Name);
                        }
                    }
                }
            }

            return result;
        }

        internal async Task RunQueryAsync(string sqlQuery)
        {
            using (DbQueryRunner runner = new DbQueryRunner(this))
            {
                await runner.RunQueryAsync(sqlQuery);
            }
        }

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
    }
}
