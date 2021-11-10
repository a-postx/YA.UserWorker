using AutoMapper;
using MediatR;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<ICommandResult<User>>
{
    public CreateUserCommand(string authId, string userId, string name, string givenName, string familyName,
        string userEmail, string picture, string nickname)
    {
        AuthId = authId;
        UserId = userId;
        Name = name;
        GivenName = givenName;
        FamilyName = familyName;
        UserEmail = userEmail;
        Picture = picture;
        Nickname = nickname;
    }

    public string AuthId { get; protected set; }
    public string UserId { get; protected set; }
    public string Name { get; protected set; }
    public string GivenName { get; protected set; }
    public string FamilyName { get; protected set; }
    public string UserEmail { get; protected set; }
    public string Picture { get; protected set; }
    public string Nickname { get; protected set; }

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, ICommandResult<User>>
    {
        public CreateUserHandler(ILogger<CreateUserHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<CreateUserHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            string authId = command.AuthId;
            string userId = command.UserId;
            string name = command.Name;
            string givenName = command.GivenName;
            string familyName = command.FamilyName;
            string userEmail = command.UserEmail;
            string picture = command.Picture;
            string nickname = command.Nickname;

            User existingUser = await _dbContext.GetUserAsync(authId, userId, cancellationToken);

            if (existingUser != null)
            {
                return new CommandResult<User>(CommandStatus.UnprocessableEntity, null);
            }

            //последний приоритет, поскольку может быть пустым для каких-то поставщиков идентификации (напр. ВКонтакте)
            string userName = userEmail;

            //для auth0, google-oauth2 и yandex
            if (!string.IsNullOrEmpty(name))
            {
                userName = name;
            }

            // для vkontakte
            if (authId == "vkontakte" && !string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
            {
                userName = givenName + " " + familyName;
            }

            User user = new User
            {
                Name = userName,
                Email = userEmail,
                AuthProvider = authId,
                ExternalId = userId,
                Picture = picture,
                Nickname = nickname,
                Settings = new UserSetting { ShowGettingStarted = true }
            };
                
            await _dbContext.CreateUserAsync(user, cancellationToken);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult<User>(CommandStatus.Ok, user);
        }
    }
}
