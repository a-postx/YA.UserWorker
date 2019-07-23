using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Models;

namespace YA.TenantWorker.DAL
{
    public interface ITenantManagerDbContext
    {
        Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken);
        void DeleteTenant(Tenant tenant);
        void UpdateTenant(Tenant tenant);
        Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken);
        Task<Tenant> GetTenantAsync(Guid? correlationId, CancellationToken cancellationToken);
        Task<ICollection<Tenant>> GetTenantsPagedAsync(int page, int count);

        Task CreateUserAsync(User user, CancellationToken cancellationToken);
        Task<User> GetUserAsync(Tenant tenant, string userName, CancellationToken cancellationToken);

        Task<ICollection<T>> GetItemsPaged<T>(
            Expression<Func<T, byte[]>> orderPredicate,
            Expression<Func<T, bool>> wherePredicate,            
            int page, int count) where T : class;

        Task<ICollection<T>> GetItemsPaged<T>(Tenant tenant, int page, int count) where T : class, ITenantEntity;

        Task<(int totalCount, int totalPages)> GetTotalPagesAsync<T>(int count, CancellationToken cancellationToken) where T : class;
        
        int ApplyChanges();
        Task<int> ApplyChangesAsync(CancellationToken cancellationToken);
    }
}
