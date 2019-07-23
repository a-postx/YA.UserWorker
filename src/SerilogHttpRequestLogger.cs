using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace YA.TenantWorker
{
    /// <summary>
    /// Middleware to log HTTP requests and exceptions via Serilog. 
    /// </summary>
    public class SerilogHttpRequestLogger
    {
        readonly RequestDelegate _next;

        public SerilogHttpRequestLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //logz.io/logstash fields can accept only 32k strings so request/response bodies are cut
            const int MaxLogFieldLength = 32766;

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            LogContext.PushProperty("UserName", httpContext.User.Identity.Name);

            // Getting the request body is a little tricky because it's a stream
            // So, we need to read the stream and then rewind it back to the beginning
            string rawRequestBody = "";
            httpContext.Request.EnableRewind();
            Stream body = httpContext.Request.Body;
            byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            rawRequestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = body;

            Log.ForContext("RequestHeaders", httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
               .ForContext("RequestBody", rawRequestBody)
               .Information("Request information {RequestMethod} {RequestPath} information", httpContext.Request.Method, httpContext.Request.Path);


            // The reponse body is also a stream so we need to:
            // - hold a reference to the original response body stream
            // - re-point the response body to a new memory stream
            // - read the response body after the request is handled into our memory stream
            // - copy the response in the memory stream out to the original response stream
            using (MemoryStream responseBodyMemoryStream = new MemoryStream())
            {
                Stream originalResponseBodyReference = httpContext.Response.Body;
                httpContext.Response.Body = responseBodyMemoryStream;

                try
                {
                    await _next(httpContext);
                }
                catch (Exception exception)
                {
                    Guid errorId = Guid.NewGuid();
                    Log.ForContext("Type", "Error")
                        .ForContext("Exception", exception, destructureObjects: true)
                        .Error(exception, "{Message}. {@ErrorId}", exception.Message, errorId);

                    string result = JsonConvert.SerializeObject(new { error = "Sorry, an unexpected error has occurred", errorId = errorId });
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = 500;

                    await httpContext.Response.WriteAsync(result);
                }

                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                string rawResponseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

                string endRequestBody = null;
                string endResponseBody = null;

                if (rawRequestBody.Length > MaxLogFieldLength)
                {
                    endRequestBody = rawRequestBody.Substring(0, MaxLogFieldLength);
                }

                if (rawResponseBody.Length > MaxLogFieldLength)
                {
                    endResponseBody = rawResponseBody.Substring(0, MaxLogFieldLength);
                }

                Log.ForContext("RequestBody", endRequestBody)
                   .ForContext("ResponseBody", endResponseBody)
                   .Information("Response information {RequestMethod} {RequestPath} {StatusCode}", httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode);

                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference);
            }
        }
    }
}
