using System;
using System.Collections.Generic;

namespace YA.UserWorker.Core.Entities
{
    public class PricingTier : IRowVersionedEntity, IUserAuditedEntity, IAuditedEntityBase
    {
        public Guid PricingTierID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool HasTrial { get; set; }
        public TimeSpan? TrialPeriod { get; set; }
        public int MaxUsers { get; set; }
        public int MaxVkPeriodicParsingTasks { get; set; }
        public virtual ICollection<Tenant> Tenants { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public byte[] tstamp { get; set; }
    }
}
