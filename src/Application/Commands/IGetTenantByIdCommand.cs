using Delobytes.AspNetCore;
using System;

namespace YA.TenantWorker.Application.Commands
{
    public interface IGetTenantByIdCommand : IAsyncCommand<Guid>
    {

    }
}
