﻿namespace YA.TenantWorker
{
    public class KeyVaultSecrets
    {
        public string LogzioToken { get; set; }
        public string AppInsightsInstrumentationKey { get; set; }
        public string MessageBusHost { get; set; }
        public string MessageBusVHost { get; set; }
        public string MessageBusLogin { get; set; }
        public string MessageBusPassword { get; set; }
        public string TenantManagerConnStr { get; set; }
    }
}
