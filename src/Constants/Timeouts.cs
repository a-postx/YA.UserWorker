namespace YA.TenantWorker.Constants
{
    public static class Timeouts
    {
        public const int HostShutdownTimeoutSec = 15;
        public const int WebHostShutdownTimeoutSec = 120;
        public const int SqlCommandTimeoutSec = 60;
        public const int RuntimeGeoDetectionTimeoutSec = 10;
        public const int ApiRequestFilterMs = 60000;
        public const int SecurityTokenValidationSec = 10;
    }
}
