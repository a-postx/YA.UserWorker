using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using System;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.ActionHandlers.Tenants
{
    public interface IPatchTenantByIdAh : IAsyncCommand<Guid, JsonPatchDocument<TenantSm>>
    {

    }
}
