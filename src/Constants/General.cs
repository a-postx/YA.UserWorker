namespace YA.TenantWorker.Constants
{
    public static class General
    {
        public const string ProductionKeyVault = "https://c934749a-91be-256-prd-kv.vault.azure.net";
        public const string DevelopmentKeyVault = "https://a7267048-82be-425-dev-kv.vault.azure.net";
        public const string AppDataFolderName = "AppData";
        public const string DefaultHttpUserAgent = "YA/1.0 (2412719@mail.ru)";
        public const string AppHttpUserAgent = "YA.TenantWorker/1.0 (2412719@mail.ru)";
        public const string DefaultSqlModelChangeDateTime = "GETUTCDATE()";
        public const int MessageBusServiceHealthPort = 5672;
        public const int MessageBusServiceHealthReconnectDelayMsec = 15000;
        public const string MessageBusServiceHealthCheckName = "message_bus_service";
        public const string MessageBusServiceStartupHealthCheckName = "message_bus_service_startup";
        public const string CorrelationIdHeader = "X-Correlation-ID";
    }
}
