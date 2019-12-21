namespace YA.TenantWorker.Constants
{
    public class MbQueueNames
    {
        public static string PrivateServiceQueueName = "ya.tenantworker." + Node.Id;

        public const string MessageBusPublishQueuePrefix = "tenantworker";
    }
}
