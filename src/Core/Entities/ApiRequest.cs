using System;

namespace YA.TenantWorker.Core.Entities
{
    public class ApiRequest : IRowVersionedEntity
    {
        private ApiRequest()
        {
            
        }

        public ApiRequest(Guid clientRequestId, DateTime dateTime, string method)
        {
            ApiRequestID = clientRequestId;
            ApiRequestDateTime = dateTime;
            Method = method;
        }

        public Guid ApiRequestID { get; private set; }
        public DateTime ApiRequestDateTime { get; private set; }
        public string Method { get; set; }
        public int? ResponseStatusCode { get; private set; }
        public string ResponseBody { get; private set; }
        public byte[] tstamp { get; set; }

        public void SetResponseStatusCode(int? statusCode)
        {
            ResponseStatusCode = statusCode;
        }

        public void SetResponseBody(string response)
        {
            ResponseBody = response;
        }
    }
}
