using System;

namespace YA.UserWorker.Application.Models.Dto
{
    public class InvitationTm
    {
        public Guid TenantId { get; set; }
        public Guid YaInvitationID { get; set; }
        public string InvitedBy { get; set; }
        public string Email { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
