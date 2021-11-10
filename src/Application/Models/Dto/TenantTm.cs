using YA.UserWorker.Application.Enums;

namespace YA.UserWorker.Application.Models.Dto;

public class TenantTm
{
    public Guid TenantId { get; set; }
    public TenantType Type { get; set; }
    public TenantStatus Status { get; set; }
    public PricingTierTm PricingTier { get; set; }
}
