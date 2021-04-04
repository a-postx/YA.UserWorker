using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace YA.UserWorker.Core.Entities
{
    public interface IRootDbContext
    {
        Task CreateTenantAsync(Tenant item, CancellationToken cancellationToken);
        Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken);
        Task<Tenant> GetTenantAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken);
        Task<Tenant> GetTenantWithPricingTierAsync(Guid tenantId, CancellationToken cancellationToken);
        void UpdateTenant(Tenant item);
        void DeleteTenant(Tenant item);

        Task CreateUserAsync(User item, CancellationToken cancellationToken);

        Task CreateMembershipAsync(Membership item, CancellationToken cancellationToken);

        int ApplyChanges();
        Task<int> ApplyChangesAsync(CancellationToken cancellationToken);
    }
}
