using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.Enums
{
    public enum LogTypes
    {
        Unknown = 0,
        BackendApiRequest = 1,
        MessageBusMessage = 2
    }
}
