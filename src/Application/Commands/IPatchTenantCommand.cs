using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using System;
using YA.TenantWorker.Application.Dto.SaveModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IPatchTenantCommand : IAsyncCommand<Guid, JsonPatchDocument<TenantSm>>
    {

    }
}
