namespace YA.UserWorker.Options;

public class OauthOptions
{
    public string Authority { get; set; }
    public string OidcIssuer { get; set; }
    public string ClientId { get; set; }
    public string Audience { get; set; }
    public string AuthorizationUrl { get; set; }
    public string TokenUrl { get; set; }
    public string ApiGatewayHost { get; set; }
    public int ApiGatewayPort { get; set; }
}

