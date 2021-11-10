using System.Linq.Expressions;

namespace YA.UserWorker.Core.Entities;

public interface IUserWorkerDbContext
{
    Task CreateEntityAsync<T>(T item, CancellationToken cancellationToken) where T : class, ITenantEntity;
    Task<T> GetEntityAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class, ITenantEntity;
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

    void DeleteEntity<T>(T item) where T : class, ITenantEntity;


    Task CreateTenantAsync(Tenant item, CancellationToken cancellationToken);
    Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Tenant> GetTenantAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);
    Task<Tenant> GetTenantWithAllRelativesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Tenant> GetTenantWithPricingTierAsync(Guid tenantId, CancellationToken cancellationToken);
    void UpdateTenant(Tenant item);
    void DeleteTenant(Tenant item);

    Task CreateUserAsync(User item, CancellationToken cancellationToken);
    Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<User> GetUserAsync(string authProvider, string externalId, CancellationToken cancellationToken);
    Task<User> GetUserWithMembershipsAsync(Guid userId, CancellationToken cancellationToken);
    Task<User> GetUserWithMembershipsAsync(string authProvider, string externalId, CancellationToken cancellationToken);
    void UpdateUser(User item);
    void DeleteUser(User item);

    Task CreateInvitationAsync(YaInvitation item, CancellationToken cancellationToken);
    Task<YaInvitation> GetInvitationAsync(Expression<Func<YaInvitation, bool>> predicate, CancellationToken cancellationToken);
    void DeleteInvitation(YaInvitation item);

    Task CreateMembershipAsync(Membership item, CancellationToken cancellationToken);
    Task<Membership> GetMembershipWithUserAsync(Expression<Func<Membership, bool>> predicate, CancellationToken cancellationToken);
    void DeleteMembership(Membership item);

    Task CreateClientInfoAsync(YaClientInfo item, CancellationToken cancellationToken);

    int ApplyChanges();
    Task<int> ApplyChangesAsync(CancellationToken cancellationToken);
}
