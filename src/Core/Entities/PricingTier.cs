﻿using System;
using System.Collections.Generic;

namespace YA.TenantWorker.Core.Entities
{
    public class PricingTier : IRowVersionedEntity, IAuditedEntityBase
    {
        public Guid PricingTierID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool HasTrial { get; set; }
        public TimeSpan? TrialPeriod { get; set; }
        public int MaxUsers { get; set; }
        public int MaxVkCommunities { get; set; }
        public int MaxVkCommunitySize { get; set; }
        public int MaxScheduledTasks { get; set; }
        public virtual ICollection<Tenant> Tenants { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
