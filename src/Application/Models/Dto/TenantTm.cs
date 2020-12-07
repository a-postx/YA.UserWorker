using System;
using YA.TenantWorker.Application.Enums;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class TenantTm
    {
        public Guid TenantId { get; set; }
        public TenantType Type { get; set; }
        public TenantStatus Status { get; set; }
    }
}
