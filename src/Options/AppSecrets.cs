namespace YA.TenantWorker.Options
{
    /// <summary>
    /// Секреты приложения
    /// </summary>
    public class AppSecrets
    {
        public string ElasticSearchUrl { get; set; }
        public string ElasticSearchUser { get; set; }
        public string ElasticSearchPassword { get; set; }
        public string LogzioToken { get; set; }
        public string AppInsightsInstrumentationKey { get; set; }
        public string MessageBusHost { get; set; }
        public int MessageBusPort { get; set; }
        public string MessageBusVHost { get; set; }
        public string MessageBusLogin { get; set; }
        public string MessageBusPassword { get; set; }
        public string DistributedCacheHost { get; set; }
        public int DistributedCachePort { get; set; }
        public string DistributedCachePassword { get; set; }
        public string ApiGatewayHost { get; set; }
        public int ApiGatewayPort { get; set; }
        public string OidcProviderIssuer { get; set; }
        public string OauthImplicitAuthorizationUrl { get; set; }
        public string OauthImplicitTokenUrl { get; set; }
        public TenantWorkerSecrets TenantWorker { get; set; }
    }
}
