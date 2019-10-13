using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class Rfc7807ProblemDetails
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<int,string> Errors { get; set; }
    }
}
