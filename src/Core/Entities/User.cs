using System;
using System.Collections.Generic;

namespace YA.TenantWorker.Core.Entities
{
    public class User : ITenantEntity, IAuditedEntityBase
    {
        public Guid UserID { get; set; }
        public virtual Tenant Tenant { get; set; }
        public Guid? CorrelationId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public bool IsPendingActivation { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
