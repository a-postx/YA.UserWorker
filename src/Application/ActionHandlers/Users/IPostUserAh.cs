using Delobytes.AspNetCore;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public interface IPostUserAh : IAsyncCommand<AccessInfoSm>
    {

    }
}
