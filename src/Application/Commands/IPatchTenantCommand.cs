﻿using Delobytes.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using System;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IPatchTenantCommand : IAsyncCommand<JsonPatchDocument<TenantSm>>
    {

    }
}
