using System;
using System.Collections.Generic;
using YA.TenantWorker.Application.Interfaces;

namespace YA.TenantWorker.Application.Models.Service
{
    /// <summary>
    /// Модель АПИ-запроса и результата.
    /// </summary>
    public class ApiRequest : ICacheable
    {
        private ApiRequest() { }

        public ApiRequest(Guid clientRequestId)
        {
            if (clientRequestId == Guid.Empty)
            {
                throw new ArgumentException("Client request ID cannot be empty", nameof(clientRequestId));
            }

            ApiRequestID = clientRequestId;

            CacheKey = $"idempotency_keys:{clientRequestId}";
            AbsoluteExpiration = new TimeSpan(24, 0, 0);
        }

        public Guid ApiRequestID { get; private set; }
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string Query { get; private set; }
        public int? StatusCode { get; private set; }
        public Dictionary<string, List<string>> Headers { get; private set; }
        public string Body { get; private set; }

        //https://github.com/ikyriak/IdempotentAPI/blob/191d84109d8860da5e96b2f9a6d20ebd1b24c228/src/Core/Idempotency.cs
        public string ResultType { get; private set; }
        public object ResultValue { get; private set; }
        public string ResultRouteName { get; private set; }
        public Dictionary<string, string> ResultRouteValues { get; private set; }

        public string CacheKey { get; }
        public TimeSpan AbsoluteExpiration { get; }

        public void SetMethod(string method)
        {
            Method = method;
        }

        public void SetPath(string path)
        {
            Path = path;
        }

        public void SetQuery(string query)
        {
            Query = query;
        }

        public void SetStatusCode(int? statusCode)
        {
            StatusCode = statusCode;
        }

        public void SetHeaders(Dictionary<string, List<string>> headers)
        {
            Headers = headers;
        }

        public void SetBody(string body)
        {
            Body = body;
        }

        public void SetResultType(string resultType)
        {
            ResultType = resultType;
        }

        public void SetResultValue(object resultValue)
        {
            ResultValue = resultValue;
        }

        public void SetResultRouteName(string resultRouteName)
        {
            ResultRouteName = resultRouteName;
        }

        public void SetResultRouteValues(Dictionary<string, string> resultRouteValues)
        {
            ResultRouteValues = resultRouteValues;
        }
    }
}
