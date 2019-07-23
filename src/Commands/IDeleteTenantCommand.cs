using System;
using Delobytes.AspNetCore;

namespace YA.TenantWorker.Commands
{
    public interface IDeleteTenantCommand : IAsyncCommand<Guid>
    {

    }
}
