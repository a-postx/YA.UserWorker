using Delobytes.AspNetCore;
using YA.UserWorker.Application.Models.HttpQueryParams;

namespace YA.UserWorker.Application.ActionHandlers.Tenants
{
    public interface IGetTenantAllPageAh : IAsyncCommand<PageOptionsCursor>
    {

    }
}
