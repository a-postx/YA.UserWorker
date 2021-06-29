using System;
using System.Threading;
using System.Threading.Tasks;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IAuthProviderManager
    {
        Task<Guid> GetUserTenantAsync(string userId, CancellationToken cancellationToken);
        Task SetTenantAsync(string userId, Guid tenantId, YaMembershipAccessType accessType, CancellationToken cancellationToken);
        Task RemoveTenantAsync(string userId, CancellationToken cancellationToken);
    }
}
