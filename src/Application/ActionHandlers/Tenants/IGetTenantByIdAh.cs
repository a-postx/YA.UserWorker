using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public interface IGetTenantByIdAh : IAsyncCommand<Guid>
{

}
