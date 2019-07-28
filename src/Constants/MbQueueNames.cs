using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Constants
{
    public class MbQueueNames
    {
        public static string PrivateServiceQueueName = "ya.tenantworker." + Program.NodeId;

        public const string MessageBusPublishQueuePrefix = "tenantworker";
    }
}
