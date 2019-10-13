using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApiErrorTypes
    {
        Unknown = 0,
        Exception = 1,
        Error = 2
    }
}
