using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IPatchTenantAh : IAsyncCommand<JsonPatchDocument<TenantSm>>
    {

    }
}
