using Delobytes.AspNetCore;
using YA.TenantWorker.Application.Dto.SaveModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IPostTenantCommand : IAsyncCommand<TenantSm>
    {

    }
}
