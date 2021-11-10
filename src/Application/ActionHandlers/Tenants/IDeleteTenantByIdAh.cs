using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public interface IDeleteTenantByIdAh : IAsyncCommand<Guid>
{

}
