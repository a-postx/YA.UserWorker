using System;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class PricingTierTm
    {
        public bool HasTrial { get; set; }
        public TimeSpan? TrialPeriod { get; set; }
        public int MaxUsers { get; set; }
        public int MaxVkCommunities { get; set; }
        public int MaxVkCommunitySize { get; set; }
        public int MaxScheduledTasks { get; set; }
    }
}
