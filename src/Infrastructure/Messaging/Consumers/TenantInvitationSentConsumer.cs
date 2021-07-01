using System;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using MbEvents;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YA.UserWorker.Application.Enums;
using YA.UserWorker.Application.Exceptions;
using YA.UserWorker.Application.Features.TenantInvitations.Commands;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.Dto;
using YA.UserWorker.Core.Entities;

namespace YA.UserWorker.Infrastructure.Messaging.Consumers
{
    public class TenantInvitationSentConsumer : IConsumer<ITenantInvitationSentV1>
    {
        public TenantInvitationSentConsumer(ILogger<TenantInvitationSentConsumer> logger,
            IMessageBus messageBus,
            IMediator mediator,
            IMapper mapper,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
        }

        private readonly ILogger<TenantInvitationSentConsumer> _log;
        private readonly IMessageBus _messageBus;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public async Task Consume(ConsumeContext<ITenantInvitationSentV1> context)
        {
            ITenantInvitationSentV1 message = context.Message;

            Guid invitationId = message.YaInvitationId;

            ICommandResult<YaInvitation> result = await _mediator
                .Send(new UpdateInvitationStatusCommand(invitationId, YaTenantInvitationStatus.Sent), _hostApplicationLifetime.ApplicationStopping);

            if (result.Status != CommandStatus.Ok)
            {
                throw new CommandExecutionException($"Cannot update tenant invitation status. Command status: {result.Status}");
            }

            InvitationTm invitationTm = _mapper.Map<InvitationTm>(result.Data);

            await _messageBus.TenantInvitationUpdatedV1Async(invitationTm, context.CancellationToken);
        }
    }
}
