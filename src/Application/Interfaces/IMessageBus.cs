using YA.UserWorker.Application.Models.Dto;

namespace YA.UserWorker.Application.Interfaces;

public interface IMessageBus
{
    Task TenantCreatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
    Task TenantUpdatedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);
    Task TenantDeletedV1Async(Guid tenantId, TenantTm tenantTm, CancellationToken cancellationToken);

    Task TenantInvitationCreatedV1Async(InvitationTm invitationTm, CancellationToken cancellationToken);
    Task TenantInvitationUpdatedV1Async(InvitationTm invitationTm, CancellationToken cancellationToken);
}
