using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.ValueObjects;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Application
{
    /// <summary>
    /// Middleware to log HTTP requests and exceptions. 
    /// </summary>
    public class HttpRequestLogger
    {
        readonly RequestDelegate _next;

        public HttpRequestLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext httpContext, ICorrelationContextAccessor correlationContextAccessor)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            string correlationId = correlationContextAccessor.CorrelationContext.CorrelationId;
            
            LogContext.PushProperty("UserName", context.User.Identity.Name);
            LogContext.PushProperty(Logs.CorrelationId, correlationId);
            LogContext.PushProperty("LogType", "request");

            context.Request.EnableRewind();
            Stream body = context.Request.Body;
            byte[] buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            string incomingRequestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            context.Request.Body = body;

            //logz.io/logstash fields can accept only 32k strings so request/response bodies are cut
            if (incomingRequestBody.Length > General.MaxLogFieldLength)
            {
                incomingRequestBody = incomingRequestBody.Substring(0, General.MaxLogFieldLength);
            }

            Log.ForContext("RequestHeaders", context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                .ForContext("RequestBody", incomingRequestBody)
                .Information("Request information {RequestMethod} {RequestPath} information", context.Request.Method, context.Request.Path);

            using (MemoryStream responseBodyMemoryStream = new MemoryStream())
            {
                Stream originalResponseBodyReference = context.Response.Body;
                context.Response.Body = responseBodyMemoryStream;
                
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    Log.Error(ex, "{ErrorMessage}", errorMessage);

                    ApiError unknownError = new ApiError(
                        ApiErrorTypes.Exception,
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        errorMessage,
                        correlationId);

                    string errorResponseBody = JsonConvert.SerializeObject(unknownError);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 500;

                    await context.Response.WriteAsync(errorResponseBody);
                }

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                if (responseBody.Contains("traceId"))
                {
                    LogContext.PushProperty(Logs.TraceId, GetTraceId(responseBody));
                }

                string endRequestBody = incomingRequestBody;
                string endResponseBody = (responseBody.Length > General.MaxLogFieldLength) ?
                    responseBody.Substring(0, General.MaxLogFieldLength) : responseBody;

                Log.ForContext("ResponseHeaders", context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                    .ForContext("ResponseBody", endResponseBody)
                    .Information("Response information {RequestMethod} {RequestPath} {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);

                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference);
            }
        }

        private static string GetTraceId(string problemDetails)
        {
            Rfc7807ProblemDetails problem = JsonConvert.DeserializeObject<Rfc7807ProblemDetails>(problemDetails);
            return problem.TraceId;
        }
    }
}
