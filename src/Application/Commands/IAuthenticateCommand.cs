using Delobytes.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Commands
{
    public interface IAuthenticateCommand : IAsyncCommand<CredentialsSm>
    {

    }
}
