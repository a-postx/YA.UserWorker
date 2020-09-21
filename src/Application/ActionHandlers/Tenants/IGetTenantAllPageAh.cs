using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.ViewModels;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IGetTenantAllPageAh : IAsyncCommand<PageOptions>
    {

    }
}
