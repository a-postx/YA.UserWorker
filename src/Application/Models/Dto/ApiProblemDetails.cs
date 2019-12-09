using Microsoft.AspNetCore.Mvc;
using System;

namespace YA.TenantWorker.Application.Models.Dto
{
    public class ApiProblemDetails : ProblemDetails
    {
        public ApiProblemDetails(string type, int? status, string instance, string title, string detail, string correlationId, string traceId = null)
        {
            Type = type;
            Status = status;
            Instance = instance;
            Title = title;
            Detail = detail;
            CorrelationID = correlationId;
            TraceId = traceId;
        }

        public string CorrelationID { get; private set; }
        public string TraceId { get; private set; }        
    }
}
