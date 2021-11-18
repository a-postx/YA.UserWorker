using AutoMapper;
using Delobytes.AspNetCore.Application;
using Delobytes.AspNetCore.Application.Commands;
using MediatR;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Application.Features.ClientInfos.Commands;

public class CreateClientInfoCommand : IRequest<ICommandResult>
{
    public CreateClientInfoCommand(ClientInfoTm clientInfoTm)
    {
        ClientInfoTm = clientInfoTm;
    }

    public ClientInfoTm ClientInfoTm { get; protected set; }

    public class CreateClientInfoHandler : IRequestHandler<CreateClientInfoCommand, ICommandResult>
    {
        public CreateClientInfoHandler(ILogger<CreateClientInfoHandler> logger,
            IMapper mapper,
            IUserWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<CreateClientInfoHandler> _log;
        private readonly IMapper _mapper;
        private readonly IUserWorkerDbContext _dbContext;

        public async Task<ICommandResult> Handle(CreateClientInfoCommand command, CancellationToken cancellationToken)
        {
            ClientInfoTm clientInfo = command.ClientInfoTm;

            YaClientInfo yaClientInfo = _mapper.Map<YaClientInfo>(clientInfo);

            await _dbContext.CreateClientInfoAsync(yaClientInfo, cancellationToken);
            await _dbContext.ApplyChangesAsync(cancellationToken);

            return new CommandResult(CommandStatus.Ok);
        }
    }
}
