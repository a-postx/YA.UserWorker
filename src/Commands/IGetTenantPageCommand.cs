using System;
using Delobytes.AspNetCore;
using YA.TenantWorker.ViewModels;

namespace YA.TenantWorker.Commands
{
    public interface IGetTenantPageCommand : IAsyncCommand<PageOptions>
    {
    }
}
