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

    /// <summary>
    /// Членство в арендаторе. Сущность не реализует ITenantEntity поскольку используется при регистрации пользователя.
    /// </summary>
    public class Membership : IUserAuditedEntity, IAuditedEntityBase, IRowVersionedEntity, ISoftDeleteEntity
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
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public byte[] tstamp { get; set; }
    }
}
