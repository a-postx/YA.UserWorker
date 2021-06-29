using Delobytes.AspNetCore;
using System;

namespace YA.UserWorker.Application.ActionHandlers.Memberships
{
    public interface IDeleteMembershipAh : IAsyncCommand<Guid>
    {

    }
}
