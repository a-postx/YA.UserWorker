using System;
using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Users
{
    public interface ISwitchUserTenantAh : IAsyncCommand<Guid>
    {

    }
}
