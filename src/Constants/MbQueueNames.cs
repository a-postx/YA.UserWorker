namespace YA.TenantWorker.Constants
{
    public static class MbQueueNames
    {
        internal static string PrivateServiceQueueName = "ya.tenantworker." + Node.Id;

        public const string MessageBusPublishQueuePrefix = "tenantworker";
        public const string PricingTierQueueName = MessageBusPublishQueuePrefix + ".pricingtier";
    }
}
