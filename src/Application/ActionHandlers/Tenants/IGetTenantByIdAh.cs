using Delobytes.AspNetCore;
using System;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IGetTenantByIdAh : IAsyncCommand<Guid>
    {

    }
}
