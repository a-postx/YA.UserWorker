namespace YA.UserWorker.Infrastructure.Authentication.Dto;

public class UserAttributes
{
    public ICollection<string> Tid { get; set; }
    public ICollection<string> TenantAccessType { get; set; }
}
