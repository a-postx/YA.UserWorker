﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
