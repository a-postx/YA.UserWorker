using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Users.Queries;

public class GetUserCommand : IRequest<ICommandResult<User>>
{
    public GetUserCommand(string authProvider, string externalId)
    {
        AuthProvider = authProvider;
        ExternalId = externalId;
    }

    public string AuthProvider { get; protected set; }
    public string ExternalId { get; protected set; }

    public class GetUserHandler : IRequestHandler<GetUserCommand, ICommandResult<User>>
    {
        public GetUserHandler(ILogger<GetUserHandler> logger,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<GetUserHandler> _log;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<User>> Handle(GetUserCommand command, CancellationToken cancellationToken)
        {
            string authProvider = command.AuthProvider;
            string externalId = command.ExternalId;

            User user = await _dbContext.GetUserWithMembershipsAsync(authProvider, externalId, cancellationToken);

            if (user == null)
            {
                return new CommandResult<User>(CommandStatus.NotFound, null);
            }

            if (user.Memberships != null && user.Memberships.Count > 0)
            {
                user.Tenants = new List<Tenant>();

                foreach (Membership membership in user.Memberships)
                {
                    Tenant tenant = await _dbContext.GetTenantWithAllRelativesAsync(membership.TenantID, cancellationToken);
                    user.Tenants.Add(tenant);
                }
            }

            return new CommandResult<User>(CommandStatus.Ok, user);
        }
    }
}
