using System;
using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Invitations
{
    public interface IDeleteInvitationAh : IAsyncCommand<Guid>
    {

    }
}
