using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Memberships;

public interface IPostMembershipAh : IAsyncCommand<Guid>
{

}
