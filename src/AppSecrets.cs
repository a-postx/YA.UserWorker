using System;

namespace YA.TenantWorker
{
    public class AppSecrets
    {
        public string LogzioToken { get; set; }
        public string AppInsightsInstrumentationKey { get; set; }
        public string MessageBusHost { get; set; }
        public string MessageBusVHost { get; set; }
        public string MessageBusLogin { get; set; }
        public string MessageBusPassword { get; set; }
        public string TenantWorkerConnStr { get; set; }
        public string ApiGatewayHost { get; set; }
        public int ApiGatewayPort { get; set; }
        [Obsolete("Jwt signing key is retrieved from OIDC provider")]
        public string JwtSigningKey { get; set; }
        public string OidcProviderIssuer { get; set; }
        public string SwaggerAuthenticationSchemeName { get; set; }
        public string OauthImplicitAuthorizationUrl { get; set; }
        public string OauthImplicitTokenUrl { get; set; }
        public string OauthImplicitClientId { get; set; }
        public string OauthImplicitResponseType { get; set; }
        public string OauthImplicitScope { get; set; }
    }
}
