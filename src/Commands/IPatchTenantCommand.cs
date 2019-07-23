using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using System;
using YA.TenantWorker.SaveModels;

namespace YA.TenantWorker.Commands
{
    public interface IPatchTenantCommand : IAsyncCommand<Guid, JsonPatchDocument<TenantSm>>
    {

    }
}
