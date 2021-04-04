using System;
using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.Tenants
{
    public interface IPatchTenantAh : IAsyncCommand<JsonPatchDocument<TenantSm>>
    {

    }
}
