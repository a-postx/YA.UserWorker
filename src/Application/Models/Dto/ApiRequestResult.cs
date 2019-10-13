using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class ApiRequestResult
    {
        public int? StatusCode { get; set; }
        public string Body { get; set; }
    }
}
