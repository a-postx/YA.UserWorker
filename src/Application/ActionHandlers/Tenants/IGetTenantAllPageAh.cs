using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IGetTenantAllPageAh : IAsyncCommand<PageOptions>
    {

    }
}
