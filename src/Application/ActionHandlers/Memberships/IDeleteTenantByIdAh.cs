using Delobytes.AspNetCore;

namespace YA.UserWorker.Application.ActionHandlers.Memberships;

public interface IDeleteMembershipAh : IAsyncCommand<Guid>
{

}
