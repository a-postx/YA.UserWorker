using Delobytes.AspNetCore;
using System;

namespace YA.UserWorker.Application.ActionHandlers.Tenants
{
    public interface IDeleteTenantByIdAh : IAsyncCommand<Guid>
    {

    }
}
