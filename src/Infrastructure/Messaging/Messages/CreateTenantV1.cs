using MbEvents;
using System;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Infrastructure.Messaging.Messages
{
    internal class CreateTenantV1 : ICreateTenantV1
    {
        internal CreateTenantV1(Guid correlationId, Guid tenantId, TenantSm tenantSm)
        {
            CorrelationId = correlationId;
            TenantId = tenantId;
            Tenant = tenantSm;
        }

        public Guid CorrelationId { get; private set; }
        public Guid TenantId { get; private set; }
        public TenantSm Tenant { get; private set; }
    }
}
