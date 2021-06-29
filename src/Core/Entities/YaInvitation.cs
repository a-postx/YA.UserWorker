using System;

namespace YA.UserWorker.Core.Entities
{
    public class YaInvitation : IRowVersionedEntity, IAuditedEntityBase
    {
        public Guid TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
        public Guid YaInvitationID { get; set; }
        public string InvitedBy { get; set; }
        public string Email { get; set; }
        public YaMembershipAccessType AccessType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool Claimed { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public Guid? CreatedMembershipId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }

        public void SetClaimed(Guid createdMembershipId)
        {
            if (createdMembershipId == Guid.Empty)
            {
                throw new ArgumentException("CreatedMembershipId is empty");
            }

            Claimed = true;
            CreatedMembershipId = createdMembershipId;
            ClaimedAt = DateTime.UtcNow;
        }
    }
}
