using System;
using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Dto.ViewModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IGetTenantPageCommand : IAsyncCommand<PageOptions>
    {
    }
}
