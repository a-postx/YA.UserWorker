using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.Users;

public interface IPatchUserAh : IAsyncCommand<JsonPatchDocument<UserSm>>
{

}
