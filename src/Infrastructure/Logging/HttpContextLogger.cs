using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Application.Models.ValueObjects;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker.Infrastructure.Logging
{
    /// <summary>
    /// HTTP request, response and exceptions logging middleware. 
    /// </summary>
    public class HttpContextLogger
    {
        public HttpContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext, ICorrelationContextAccessor correlationContextAccessor)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            
            LogContext.PushProperty(Logs.LogType, LogTypes.Request);

            string initialRequestBody = "";
            httpContext.Request.EnableBuffering();
            Stream body = httpContext.Request.Body;
            byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            initialRequestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = body;

            //logz.io/logstash fields can accept only 32k strings so request/response bodies are cut
            if (initialRequestBody.Length > General.MaxLogFieldLength)
            {
                initialRequestBody = initialRequestBody.Substring(0, General.MaxLogFieldLength);
            }

            Log.ForContext(Logs.RequestHeaders, context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                .ForContext(Logs.RequestBody, initialRequestBody)
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
                        correlationContextAccessor.CorrelationContext.CorrelationId);

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
                
                string endResponseBody = (responseBody.Length > General.MaxLogFieldLength) ?
                    responseBody.Substring(0, General.MaxLogFieldLength) : responseBody;

                Log.ForContext(Logs.ResponseHeaders, context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                    .ForContext(Logs.ResponseBody, endResponseBody)
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
