﻿using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.SaveModels;

namespace YA.TenantWorker.Application.Interfaces
{
    public interface IMessageBus
    {
        Task CreateTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken);
        Task DeleteTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken);
        Task UpdateTenantV1Async(Guid correlationId, Guid tenantId, TenantSm tenantSm, CancellationToken cancellationToken);

        Task SendPricingTierV1Async(Guid correlationId, Guid tenantId, PricingTierTm pricingTierTm, CancellationToken cancellationToken);
    }
}
