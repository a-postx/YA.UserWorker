using MbEvents;
using YA.UserWorker.Application.Models.Dto;

namespace YA.UserWorker.Infrastructure.Messaging.Messages;

internal class TenantInvitationUpdatedV1 : ITenantInvitationUpdatedV1
{
    internal TenantInvitationUpdatedV1(Guid correlationId, Guid tenantId, InvitationTm invitationTm)
    {
        CorrelationId = correlationId;
        TenantId = tenantId;
        Invitation = invitationTm;
    }

    public Guid CorrelationId { get; private set; }
    public Guid TenantId { get; private set; }
    public InvitationTm Invitation { get; private set; }
}
