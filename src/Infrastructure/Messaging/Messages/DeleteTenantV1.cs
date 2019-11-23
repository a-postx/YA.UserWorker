using MbEvents;
using System;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Infrastructure.Messaging.Messages
{
    internal class DeleteTenantV1 : IDeleteTenantV1
    {
        internal DeleteTenantV1(Guid tenantId, Guid correlationId, TenantSm tenantSm)
        {
            TenantId = tenantId;
            CorrelationId = correlationId;
            Tenant = tenantSm;
        }

        public Guid TenantId { get; private set; }
        public Guid CorrelationId { get; private set; }
        public TenantSm Tenant { get; private set; }
    }
}
