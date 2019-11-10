using System;
using System.Collections.Generic;

namespace YA.TenantWorker.Core.Entities
{
    public enum TenantTypes
    {
        System = 0,
        Custom = 1
    }
    public class Tenant : IAuditedEntityBase, IRowVersionedEntity
    {
        public Guid TenantID { get; set; }
        public TenantTypes TenantType { get; set; }
        public string TenantName { get; set; }
        public virtual PricingTier PricingTier { get; set; }
        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
        public bool IsReadOnly { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
