using System;
using MassTransit;
using YA.UserWorker.Application.Models.Dto;

namespace MbEvents
{
    public interface ITenantIdMbMessage
    {
        Guid TenantId { get; }
    }

    public interface ITenantCreatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }

    public interface ITenantDeletedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }

    public interface ITenantUpdatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        TenantTm Tenant { get; }
    }

    public interface ITenantInvitationCreatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        InvitationTm Invitation { get; }
    }

    public interface ITenantInvitationSentV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        Guid YaInvitationId { get; }
    }

    public interface ITenantInvitationUpdatedV1 : CorrelatedBy<Guid>, ITenantIdMbMessage
    {
        InvitationTm Invitation { get; }
    }
}
