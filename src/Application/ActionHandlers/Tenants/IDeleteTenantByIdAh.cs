using Delobytes.AspNetCore;
using System;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IDeleteTenantByIdAh : IAsyncCommand<Guid>
    {

    }
}
