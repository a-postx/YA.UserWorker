using System;

namespace YA.UserWorker.Core.Entities
{
    [Flags]
    public enum YaMembershipAccessType
    {
        Unknown = 0,
        ReadOnly = 1,
        ReadWrite = 2,
        Admin = 4,
        Owner = 8
    }

    public class Membership : IAuditedEntityBase, IRowVersionedEntity, ISoftDeleteEntity
    {
        public Guid MembershipID { get; set; }
        public Guid UserID { get; set; }
        public virtual User User { get; set; }
        public Guid TenantID { get; set; }
        public virtual Tenant Tenant { get; set; }
        public YaMembershipAccessType AccessType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
