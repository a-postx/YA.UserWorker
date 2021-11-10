using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.Tenants;

public interface IPatchTenantByIdAh : IAsyncCommand<Guid, JsonPatchDocument<TenantSm>>
{

}
