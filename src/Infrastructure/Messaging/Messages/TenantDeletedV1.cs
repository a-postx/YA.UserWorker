using MbEvents;
using System;
using YA.TenantWorker.Application.Models.Dto;

namespace YA.TenantWorker.Infrastructure.Messaging.Messages
{
    internal class TenantDeletedV1 : ITenantDeletedV1
    {
        internal TenantDeletedV1(Guid correlationId, Guid tenantId, TenantTm tenantTm)
        {
            CorrelationId = correlationId;
            TenantId = tenantId;            
            Tenant = tenantTm;
        }

        public Guid CorrelationId { get; private set; }
        public Guid TenantId { get; private set; }        
        public TenantTm Tenant { get; private set; }
    }
}
