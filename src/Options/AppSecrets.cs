namespace YA.UserWorker.Options;

/// <summary>
/// Секреты приложения
/// </summary>
public class AppSecrets
{
    public string ElasticSearchUrl { get; set; }
    public string ElasticSearchUser { get; set; }
    public string ElasticSearchPassword { get; set; }
    public string MessageBusHost { get; set; }
    public int MessageBusPort { get; set; }
    public string MessageBusVHost { get; set; }
    public string MessageBusLogin { get; set; }
    public string MessageBusPassword { get; set; }
    public string DistributedCacheHost { get; set; }
    public int DistributedCachePort { get; set; }
    public string DistributedCachePassword { get; set; }
    public string Auth0ManagementApiClientId { get; set; }
    public string Auth0ManagementApiClientSecret { get; set; }
    public string KeycloakManagementApiClientId { get; set; }
    public string KeycloakManagementApiClientSecret { get; set; }
    public UserWorkerSecrets UserWorker { get; set; }
}
