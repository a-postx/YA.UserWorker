namespace YA.TenantWorker.Constants
{
    public static class General
    {
        public const int SystemShutdownTimeoutSec = 3600;
        public const string ProductionKeyVault = "https://c934749a-91be-256-prd-kv.vault.azure.net";
        public const string DevelopmentKeyVault = "https://a7267048-82be-425-dev-kv.vault.azure.net";
        public const string AppDataFolderName = "AppData";
        public const string DefaultHttpUserAgent = "YA/1.0 (2412719@mail.ru)";
        public const string AppHttpUserAgent = "YA.TenantWorker/1.0 (2412719@mail.ru)";
        public const int MaxLogFieldLength = 27716;
        /// <summary>
        /// UTC kind conversion exist in EF Core so refactoring is needed in case of value change.
        /// </summary>
        public const string DefaultSqlModelDateTimeFunction = "GETUTCDATE()";
        public const int MessageBusServiceHealthPort = 5672;
        public const int MessageBusServiceHealthReconnectDelayMsec = 15000;
        public const string MessageBusServiceHealthCheckName = "message_bus_service";
        public const string SqlDatabaseHealthCheckName = "sql_database";
        public const string ForwardedForHeader = "X-Original-For";
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const int StartupServiceCheckRetryIntervalMs = 10000;
        public const int ApiRequestsCacheSize = 256;
        public const int ApiRequestCacheSlidingExpirationSec = 120;
        public const int ApiRequestCacheAbsoluteExpirationSec = 300;
        public const int DefaultPageSizeForPagination = 3;
    }
}
