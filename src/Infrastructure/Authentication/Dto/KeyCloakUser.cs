namespace YA.UserWorker.Infrastructure.Authentication.Dto;

/// <summary>
/// Пользователь (версия 17)
/// </summary>
public class KeyCloakUser
{
    public string Id { get; set; }
    public string Username { get; set; }
    public bool Enabled { get; set; }
    public string Email { get; set; }
    public bool EmailVerified { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserAttributes Attributes { get; set; }
    public bool Totp { get; set; }
    public object[] RequiredActions { get; set; }
    public int NotBefore { get; set; }
    public object[] DisableableCredentialTypes { get; set; }
    public double CreatedTimestamp { get; set; }
    public UserAccess Access { get; set; }
}
