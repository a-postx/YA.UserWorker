namespace YA.UserWorker.Infrastructure.Authentication.Dto;

public class UserAccess
{
    public bool Impersonate { get; set; }
    public bool Manage { get; set; }
    public bool ManageGroupMembership { get; set; }
    public bool MapRoles { get; set; }
    public bool View { get; set; }
}
