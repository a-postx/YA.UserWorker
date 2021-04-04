using System;

namespace YA.UserWorker.Core.Entities
{
    public enum MembershipAccessType
    {
        Unknown = 0,
        ReadOnly = 1,
        ReadWrite = 2,
        Admin = 3,
        Owner = 4
    }

    public class Membership : IAuditedEntityBase, IRowVersionedEntity, ISoftDeleteEntity
    {
        public Guid MembershipID { get; set; }
        public Guid UserID { get; set; }
        public virtual User User { get; set; }
        public Guid TenantID { get; set; }
        public virtual Tenant Tenant { get; set; }
        public MembershipAccessType AccessType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
