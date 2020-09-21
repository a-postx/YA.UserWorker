using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.ActionHandlers.ClientInfos
{
    public interface IPostClientInfoAh : IAsyncCommand<ClientInfoSm>
    {

    }
}
