using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.HttpQueryParams;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IGetTenantAllPageAh : IAsyncCommand<PageOptionsCursor>
    {

    }
}
