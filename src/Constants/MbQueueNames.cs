namespace YA.UserWorker.Constants
{
    public static class MbQueueNames
    {
        internal static string PrivateServiceQueueName = "ya.userworker." + Node.Id;

        public const string MessageBusPublishQueuePrefix = "userworker";
        public const string PricingTierQueueName = MessageBusPublishQueuePrefix + ".pricingtier";
    }
}
