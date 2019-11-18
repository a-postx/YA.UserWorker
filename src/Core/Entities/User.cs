using System;
using System.Collections.Generic;

namespace YA.TenantWorker.Core.Entities
{
    public class User : ITenantEntity, IRowVersionedEntity, IAuditedEntityBase
    {
        public virtual Tenant Tenant { get; set; }
        public Guid UserID { get; set; }        
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsPendingActivation { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
