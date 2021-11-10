using Delobytes.AspNetCore;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.ClientInfos;

public interface IPostClientInfoAh : IAsyncCommand<ClientInfoSm>
{

}
