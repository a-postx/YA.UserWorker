using System;
using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IGetTenantPageCommand : IAsyncCommand<PageOptions>
    {
    }
}
