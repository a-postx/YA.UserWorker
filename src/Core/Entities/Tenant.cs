using System;
using System.Collections.Generic;

namespace YA.TenantWorker.Core.Entities
{
    public enum TenantType
    {
        System = 0,
        Custom = 1
    }

    public enum TenantStatus
    {
        New = 0,
        Activated = 1
    }

    public class Tenant : IRowVersionedEntity, IAuditedEntityBase
    {
        public Guid TenantID { get; set; }
        public string Name { get; set; }
        public TenantType Type { get; set; }
        public string Email { get; set; }
        public string AuthProvider { get; set; }
        public string ExternalId { get; set; }
        public Guid PricingTierId { get; set; }
        public virtual PricingTier PricingTier { get; set; }
        public DateTime PricingTierActivatedDateTime { get; set; }
        public DateTime PricingTierActivatedUntilDateTime { get; set; }
        public TenantStatus Status { get; set; }
        public bool IsReadOnly { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
