using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using YA.UserWorker.Application.Models.SaveModels;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<ICommandResult<User>>
{
    public UpdateUserCommand(string userProvider, string userId, JsonPatchDocument<UserSm> patch)
    {
        UserProvider = userProvider;
        UserId = userId;
        Patch = patch;
    }

    public string UserProvider { get; protected set; }
    public string UserId { get; protected set; }
    public JsonPatchDocument<UserSm> Patch { get; protected set; }

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ICommandResult<User>>
    {
        public UpdateUserHandler(ILogger<UpdateUserHandler> logger,
            IMapper mapper,
            IValidator<UserSm> validator,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<UpdateUserHandler> _log;
        private readonly IMapper _mapper;
        private readonly IValidator<UserSm> _validator;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<User>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            string userProvider = command.UserProvider;
            string userId = command.UserId;
            JsonPatchDocument<UserSm> patch = command.Patch;

            if (patch == null)
            {
                return new CommandResult<User>(CommandStatus.BadRequest, null);
            }

            User user = await _dbContext.GetUserWithMembershipsAsync(userProvider, userId, cancellationToken);

            if (user == null)
            {
                return new CommandResult<User>(CommandStatus.NotFound, null);
            }

            UserSm userSm = _mapper.Map<UserSm>(user);

            patch.ApplyTo(userSm);

            ValidationResult validationResult = _validator.Validate(userSm);

            if (!validationResult.IsValid)
            {
                return new CommandResult<User>(CommandStatus.ModelInvalid, null, validationResult.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            user = (User)_mapper.Map(userSm, user, typeof(UserSm), typeof(User));

            _dbContext.UpdateUser(user);
            await _dbContext.ApplyChangesAsync(cancellationToken);

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
