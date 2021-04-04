using System;

namespace YA.UserWorker.Application.Models.Dto
{
    public class PricingTierTm
    {
        public Guid PricingTierID { get; set; }
        public bool HasTrial { get; set; }
        public TimeSpan? TrialPeriod { get; set; }
        public int MaxUsers { get; set; }
        public int MaxVkCommunities { get; set; }
        public int MaxVkCommunitySize { get; set; }
        public int MaxScheduledTasks { get; set; }
    }
}
