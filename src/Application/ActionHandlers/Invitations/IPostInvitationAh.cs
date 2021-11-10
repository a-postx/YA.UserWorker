using Delobytes.AspNetCore;
using YA.UserWorker.Application.Models.SaveModels;

namespace YA.UserWorker.Application.ActionHandlers.Invitations;

public interface IPostInvitationAh : IAsyncCommand<InvitationSm>
{

}
