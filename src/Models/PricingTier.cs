using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Models
{
    public class PricingTier
    {
        public Guid PricingTierID { get; set; }
        public Guid? CorrelationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ResourceLevels { get; set; }
        public string Features { get; set; }
        public virtual ICollection<Tenant> Tenants { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
