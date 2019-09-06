﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Core.Entities
{
    public interface IAuditedEntityBase
    {
        DateTime LastModifiedDateTime { get; set; }
    }
}
