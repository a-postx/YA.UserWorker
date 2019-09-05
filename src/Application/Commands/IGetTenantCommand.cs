using System;
using Delobytes.AspNetCore;

namespace YA.TenantWorker.Application.Commands
{
    public interface IGetTenantCommand : IAsyncCommand<Guid>
    {

    }
}
