using System;
using Delobytes.AspNetCore;

namespace YA.TenantWorker.Application.Commands
{
    public interface IDeleteTenantByIdCommand : IAsyncCommand<Guid>
    {

    }
}
