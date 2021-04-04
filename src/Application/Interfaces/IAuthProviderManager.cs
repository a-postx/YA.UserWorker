using System;
using System.Threading;
using System.Threading.Tasks;

namespace YA.UserWorker.Application.Interfaces
{
    public interface IAuthProviderManager
    {
        Task SetTenantIdAsync(string userId, Guid tenantId, CancellationToken cancellationToken);
        Task RemoveTenantIdAsync(string userId, CancellationToken cancellationToken);
    }
}
