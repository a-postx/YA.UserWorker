using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IPostTenantCommand : IAsyncCommand<TenantSm>
    {

    }
}
