using System.Text.Json.Serialization;

namespace YA.UserWorker.Infrastructure.Authentication.Dto;

public class KeyCloakApiManagementTokenResponse
{
    public string Access_token { get; set; }
    public string Token_type { get; set; }
    public int Expires_in { get; set; }
    public int Refresh_expires_in { get; set; }
    [JsonPropertyName("not-before-policy")]
    public int Not_before_policy { get; set; }
    public string Scope { get; set; }
}
