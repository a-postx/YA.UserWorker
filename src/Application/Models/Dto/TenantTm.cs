using System;
using YA.TenantWorker.Application.Enums;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class TenantTm
    {
        public Guid TenantId { get; set; }
        public TenantTypes Type { get; set; }
        public TenantStatuses Status { get; set; }
    }
}
