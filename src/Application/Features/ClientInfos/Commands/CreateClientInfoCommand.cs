using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Features.ClientInfos.Commands
{
    public class CreateClientInfoCommand : IRequest<ICommandResult<EmptyCommandResult>>
    {
        public CreateClientInfoCommand(ClientInfoTm clientInfoTm)
        {
            ClientInfoTm = clientInfoTm;
        }

        public ClientInfoTm ClientInfoTm { get; protected set; }

        public class CreateClientInfoHandler : IRequestHandler<CreateClientInfoCommand, ICommandResult<EmptyCommandResult>>
        {
            public CreateClientInfoHandler(ILogger<CreateClientInfoHandler> logger,
                IMapper mapper,
                ITenantWorkerDbContext dbContext)
            {
                _log = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            }

            private readonly ILogger<CreateClientInfoHandler> _log;
            private readonly IMapper _mapper;
            private readonly ITenantWorkerDbContext _dbContext;

            public async Task<ICommandResult<EmptyCommandResult>> Handle(CreateClientInfoCommand command, CancellationToken cancellationToken)
            {
                ClientInfoTm clientInfo = command.ClientInfoTm;

                YaClientInfo yaClientInfo = _mapper.Map<YaClientInfo>(clientInfo);

                await _dbContext.CreateEntityAsync(yaClientInfo, cancellationToken);
                await _dbContext.ApplyChangesAsync(cancellationToken);

                return new CommandResult<EmptyCommandResult>(CommandStatuses.Ok, null);
            }
        }
    }
}
