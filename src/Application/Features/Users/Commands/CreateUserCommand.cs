using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Constants;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.Users.Commands
{
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
                IUserWorkerDbContext dbContext,
                IMessageBus messageBus)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
                _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            }

            private readonly ILogger<CreateUserHandler> _log;
            private readonly IMapper _mapper;
            private readonly IUserWorkerDbContext _dbContext;
            private readonly IMessageBus _messageBus;

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

                string userName = userEmail;

                if (authId == "auth0" && !string.IsNullOrEmpty(name))
                {
                    userName = name;
                }

                if (authId == "vkontakte" && name.Contains("id", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
                {
                    userName = givenName + " " + familyName;
                }

                if (authId == "yandex" && !string.IsNullOrEmpty(name))
                {
                    userName = name;
                }

                if (authId == "google-oauth2" && !string.IsNullOrEmpty(name))
                {
                    userName = name;
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

                Tenant tenant = new Tenant
                {
                    Name = string.IsNullOrEmpty(userEmail) ? "<неизвестный>" : userEmail,
                    Status = YaTenantStatus.New,
                    Type = YaTenantType.Custom
                };

                Guid defaultPricingTierId = Guid.Parse(SeedData.SeedPricingTierId);

                tenant.PricingTierId = defaultPricingTierId;
                tenant.PricingTierActivatedDateTime = DateTime.UtcNow;

                await _dbContext.CreateTenantAsync(tenant, cancellationToken);

                //необходимо получить идентификаторы сущностей
                await _dbContext.ApplyChangesAsync(cancellationToken);

                Membership membership = new Membership
                {
                    UserID = user.UserID,
                    TenantID = tenant.TenantID,
                    AccessType = YaMembershipAccessType.Owner
                };

                await _dbContext.CreateMembershipAsync(membership, cancellationToken);

                await _dbContext.ApplyChangesAsync(cancellationToken);

                await _messageBus.TenantCreatedV1Async(tenant.TenantID, _mapper.Map<TenantTm>(tenant), cancellationToken);

                user.Tenants = new List<Tenant>
                {
                    tenant
                };

                return new CommandResult<User>(CommandStatus.Ok, user);
            }
        }
    }
}
