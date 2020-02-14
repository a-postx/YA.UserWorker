using System;
using YA.TenantWorker.Application.Enums;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class TenantTm
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public TenantTypes TenantType { get; set; }
    }
}
