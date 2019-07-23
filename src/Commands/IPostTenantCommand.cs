using Delobytes.AspNetCore;
using YA.TenantWorker.SaveModels;

namespace YA.TenantWorker.Commands
{
    public interface IPostTenantCommand : IAsyncCommand<TenantSm>
    {

    }
}
