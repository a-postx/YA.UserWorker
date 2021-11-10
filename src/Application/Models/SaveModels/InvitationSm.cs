using YA.UserWorker.Application.Enums;

namespace YA.UserWorker.Application.Models.SaveModels;

public class InvitationSm
{
    public string Email { get; set; }
    public MembershipAccessType AccessType { get; set; }
    public string InvitedBy { get; set; }
}
