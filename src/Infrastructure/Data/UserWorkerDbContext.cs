using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using YA.Common;
using YA.Common.Extensions;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;
using YA.UserWorker.Extensions;

namespace YA.UserWorker.Infrastructure.Data;

/// <summary>
/// Контекст работы с базой данных приложения.
/// ПРЕДУПРЕЖДЕНИЕ БЕЗОПАСНОСТИ: присутствуют сущности без глобальных фильтров.
/// </summary>
public class UserWorkerDbContext : DbContext, IUserWorkerDbContext
{
    public UserWorkerDbContext(DbContextOptions options, IRuntimeContextAccessor runtimeCtx) : base(options)
    {
        ArgumentNullException.ThrowIfNull(runtimeCtx);

        _tenantId = runtimeCtx.GetTenantId();
        _userId = runtimeCtx.GetUserId();
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PricingTier> PricingTiers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<YaInvitation> Invitations { get; set; }
    public DbSet<YaClientInfo> ClientInfos { get; set; }

    private readonly Guid _tenantId;
    private readonly string _userId;

    private IDbContextTransaction _currentTransaction;

    public IDbContextTransaction CurrentTransaction
    {
        get
        {
            return _currentTransaction;
        }
    }

    public bool HasActiveTransaction => _currentTransaction != null;
    private static bool IsMustHaveTenantFilterEnabled => true;
    private static bool IsSoftDeleteFilterEnabled => true;

    private static readonly MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(UserWorkerDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
            
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserWorkerDbContext).Assembly);

        modelBuilder.Seed();
            
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            ConfigureGlobalFiltersMethodInfo
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { modelBuilder, entityType });
        }
    }

    #region Generics
    public async Task CreateEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class, ITenantEntity
    {
        await Set<T>().AddAsync(item, cancellationToken);
    }

    public async Task<T> GetEntityAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity
    {
        return await Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

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
        return Task.FromResult(Set<T>()
            .OrderBy(t => t.CreatedDateTime)
            .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
            .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
            .If(first.HasValue, x => x.Take(first.Value))
            .ToList());
    }

    public Task<List<T>> GetEntitiesPagedReverseAsync<T>(int? last, DateTimeOffset? createdAfter,
        DateTimeOffset? createdBefore, bool orderDesc, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
    {
        return Task.FromResult(Set<T>()
            .IfElse(orderDesc,
                x => x.OrderByDescending(t => t.CreatedDateTime),
                x => x.OrderBy(t => t.CreatedDateTime))
            .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
            .If(createdBefore.HasValue, x => x.Where(y => y.CreatedDateTime < createdBefore.Value))
            //применяем хак, TakeLast не работает в EF - "This overload of the method 'System.Linq.Queryable.TakeLast' is currently not supported." in v.4.2.2.0
            .If(last.HasValue, x =>
                x.IfElse(orderDesc,
                    y => y.OrderBy(t => t.CreatedDateTime).Take(last.Value).OrderByDescending(t => t.CreatedDateTime),
                    y => y.OrderByDescending(t => t.CreatedDateTime).Take(last.Value).OrderBy(t => t.CreatedDateTime)))
            ////x.TakeLast(last.Value))
            .ToList());
    }

    public Task<bool> GetEntitiesPagedHasNextPageAsync<T>(int? first, DateTimeOffset? createdAfter, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
    {
        return Task.FromResult(Set<T>()
            .OrderBy(t => t.CreatedDateTime)
            .If(createdAfter.HasValue, x => x.Where(y => y.CreatedDateTime > createdAfter.Value))
            .Skip(first.Value)
            .Any());
    }

    public Task<bool> GetEntitiesPagedHasPreviousPageAsync<T>(int? last, DateTimeOffset? createdBefore,
        bool orderDesc, CancellationToken cancellationToken) where T : class, IAuditedEntityBase, IRowVersionedEntity
    {
        return Task.FromResult(Set<T>()
            .IfElse(orderDesc,
                x => x.OrderByDescending(t => t.CreatedDateTime),
                x => x.OrderBy(t => t.CreatedDateTime))
            .If(createdBefore.HasValue, x =>
                x.IfElse(orderDesc,
                    y => y.Where(y => y.CreatedDateTime > createdBefore.Value),
                    y => y.Where(y => y.CreatedDateTime < createdBefore.Value)))
            //применяем хак, TakeLast не работает в EF - "This overload of the method 'System.Linq.Queryable.TakeLast' is currently not supported." in v.4.2.2.0
            .IfElse(orderDesc,
                x => x.OrderBy(y => y.CreatedDateTime).Skip(last.Value).OrderByDescending(t => t.CreatedDateTime),
                x => x.OrderByDescending(y => y.CreatedDateTime).Skip(last.Value).OrderBy(t => t.CreatedDateTime))
            ////.SkipLast(last.Value)
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
            getTenantsTask = GetEntitiesPagedReverseAsync<T>(last, createdAfter, createdBefore, false, cancellationToken);
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
            return await GetEntitiesPagedHasPreviousPageAsync<T>(last, createdBefore, false, cancellationToken);
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

    public void DeleteEntity<T>(T item) where T : class, ITenantEntity
    {
        Set<T>().Remove(item);
    }
    #endregion

    #region Tenants
    public async Task CreateTenantAsync(Tenant item, CancellationToken cancellationToken)
    {
        await Set<Tenant>().AddAsync(item, cancellationToken);
    }

    public async Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await Tenants.SingleOrDefaultAsync(e => e.TenantID == tenantId, cancellationToken);
    }

    public async Task<Tenant> GetTenantAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Set<Tenant>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Tenant> GetTenantWithAllRelativesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await Set<Tenant>()
            .Include(nameof(PricingTier))
            .Include(e => e.Memberships)
                .ThenInclude(c => c.User)
            .Include(e => e.Invitations)
            .SingleOrDefaultAsync(e => e.TenantID == tenantId, cancellationToken);
    }

    public async Task<Tenant> GetTenantWithPricingTierAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await Set<Tenant>()
            .Include(nameof(PricingTier))
            .SingleOrDefaultAsync(e => e.TenantID == tenantId, cancellationToken);
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

    #region Users
    public async Task CreateUserAsync(User item, CancellationToken cancellationToken)
    {
        await Set<User>().AddAsync(item, cancellationToken);
    }

    public async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await Users.SingleOrDefaultAsync(e => e.UserID == userId, cancellationToken);
    }

    public async Task<User> GetUserAsync(string authProvider, string externalId, CancellationToken cancellationToken)
    {
        return await Users
            .SingleOrDefaultAsync(e => e.AuthProvider == authProvider && e.ExternalId == externalId, cancellationToken);
    }

    public async Task<User> GetUserWithMembershipsAsync(string authProvider, string externalId, CancellationToken cancellationToken)
    {
        return await Users
            .Include(e => e.Memberships)
            .SingleOrDefaultAsync(e => e.AuthProvider == authProvider && e.ExternalId == externalId, cancellationToken);
    }

    public async Task<User> GetUserWithMembershipsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await Set<User>()
            .Include(e => e.Memberships)
            .SingleOrDefaultAsync(e => e.UserID == userId, cancellationToken);
    }

    public void UpdateUser(User item)
    {
        Set<User>().Update(item);
    }

    public void DeleteUser(User item)
    {
        Set<User>().Remove(item);
    }
    #endregion

    #region TenantInvitations
    public async Task CreateInvitationAsync(YaInvitation item, CancellationToken cancellationToken)
    {
        await Set<YaInvitation>().AddAsync(item, cancellationToken);
    }

    public async Task<YaInvitation> GetInvitationAsync(Expression<Func<YaInvitation, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Set<YaInvitation>().Include(e => e.Tenant).SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public void DeleteInvitation(YaInvitation item)
    {
        Set<YaInvitation>().Remove(item);
    }
    #endregion

    #region Memberships
    public async Task CreateMembershipAsync(Membership item, CancellationToken cancellationToken)
    {
        await Set<Membership>().AddAsync(item, cancellationToken);
    }

    public async Task<Membership> GetMembershipWithUserAsync(Expression<Func<Membership, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Set<Membership>()
            .Include(e => e.User)
            .SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public void DeleteMembership(Membership item)
    {
        Set<Membership>().Remove(item);
    }
    #endregion

    public async Task CreateClientInfoAsync(YaClientInfo item, CancellationToken cancellationToken)
    {
        await Set<YaClientInfo>().AddAsync(item, cancellationToken);
    }

    #region Filtering
    private void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType) where TEntity : class
    {
        if (entityType.BaseType == null && ShouldFilterEntity<TEntity>())
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

    private bool ShouldFilterEntity<TEntity>() where TEntity : class
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
            Expression<Func<TEntity, bool>> mustHaveTenantFilter = e
                => !IsMustHaveTenantFilterEnabled || ((ITenantEntity)e).Tenant.TenantID == _tenantId;
            expression = mustHaveTenantFilter;
        }

        if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> softDeleteFilter = e => !IsSoftDeleteFilterEnabled || !((ISoftDeleteEntity)e).IsDeleted;
#pragma warning disable CA1508 // выражение может быть пустым
            expression = expression == null ? softDeleteFilter : CombineExpressions(expression, softDeleteFilter);
#pragma warning restore CA1508
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
        SetCreationTenantProperties(entry.Entity);
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

    private void SetCreationTenantProperties(object entityAsObj)
    {
        if (entityAsObj is ITenantEntity)
        {
            ITenantEntity tenantEntity = entityAsObj.As<ITenantEntity>();
            tenantEntity.TenantId = _tenantId;
        }
    }

    private void SetCreationAuditProperties(object entityAsObj)
    {
        if (entityAsObj is IAuditedEntityBase)
        {
            IAuditedEntityBase auditedEntity = entityAsObj.As<IAuditedEntityBase>();
            auditedEntity.LastModifiedDateTime = DateTime.UtcNow;
        }

        if (entityAsObj is IUserAuditedEntity)
        {
            IUserAuditedEntity userAuditedEntity = entityAsObj.As<IUserAuditedEntity>();
            userAuditedEntity.CreatedBy = _userId;
        }
    }

    private void SetModificationAuditProperties(object entityAsObj)
    {
        if (entityAsObj is IAuditedEntityBase)
        {
            IAuditedEntityBase auditedEntity = entityAsObj.As<IAuditedEntityBase>();
            auditedEntity.LastModifiedDateTime = DateTime.UtcNow;
        }

        if (entityAsObj is IUserAuditedEntity)
        {
            IUserAuditedEntity userAuditedEntity = entityAsObj.As<IUserAuditedEntity>();
            userAuditedEntity.LastModifiedBy = _userId;
        }
    }

    private void SetDeletionAuditProperties(object entityAsObj)
    {
        // модифицируем для мягко удалённых сущностей
        if (entityAsObj is IAuditedEntityBase)
        {
            entityAsObj.As<IAuditedEntityBase>().LastModifiedDateTime = DateTime.UtcNow;
        }

        if (entityAsObj is IUserAuditedEntity)
        {
            IUserAuditedEntity userAuditedEntity = entityAsObj.As<IUserAuditedEntity>();
            userAuditedEntity.LastModifiedBy = _userId;
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
                throw new SecurityException("More than one TenantID detected.");
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
        if (_currentTransaction != null)
        {
            return null;
        }

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (transaction != _currentTransaction)
        {
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
        }

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
