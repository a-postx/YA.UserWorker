using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using YA.Common.Constants;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Прослойка логирования HTTP-контекста - запросов и ответов. 
    /// </summary>
    public class HttpContextLogger
    {
        public HttpContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext,
            IHostApplicationLifetime lifetime,
            IOptions<GeneralOptions> options)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            int maxLogFieldLength = options.Value.MaxLogFieldLength;

            if (context.Request.Path == "/metrics")
            {
                using (LogContext.PushProperty(YaLogKeys.LogType, LogTypes.MetricRequest.ToString()))
                {
                    await _next(context);
                }
            }
            else
            {
                using (LogContext.PushProperty(YaLogKeys.LogType, LogTypes.ApiRequest.ToString()))
                {
                    httpContext.Request.EnableBuffering();
                    Stream body = httpContext.Request.Body;
                    byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength, CultureInfo.InvariantCulture)];
                    await httpContext.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), lifetime.ApplicationStopping);
                    string initialRequestBody = Encoding.UTF8.GetString(buffer);
                    body.Seek(0, SeekOrigin.Begin);
                    httpContext.Request.Body = body;

                    //logz.io/logstash fields can accept only 32k strings so request/response bodies are cut
                    if (initialRequestBody.Length > maxLogFieldLength)
                    {
                        initialRequestBody = initialRequestBody.Substring(0, maxLogFieldLength);
                    }

                    //у МС нет автоматического деструктурирования, поэтому используем Серилог ценой дырки в абстрации
                    Log.ForContext(YaLogKeys.RequestHeaders, context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                        .ForContext(YaLogKeys.RequestBody, initialRequestBody)
                        .ForContext(YaLogKeys.RequestProtocol, context.Request.Protocol)
                        .ForContext(YaLogKeys.RequestScheme, context.Request.Scheme)
                        .ForContext(YaLogKeys.RequestHost, context.Request.Host.Value)
                        .ForContext(YaLogKeys.RequestMethod, context.Request.Method)
                        .ForContext(YaLogKeys.RequestPath, context.Request.Path)
                        .ForContext(YaLogKeys.RequestQuery, context.Request.QueryString)
                        .ForContext(YaLogKeys.RequestPathAndQuery, GetFullPath(context))
                        .Information("HTTP request received.");

                    using (MemoryStream responseBodyMemoryStream = new MemoryStream())
                    {
                        Stream originalResponseBodyReference = context.Response.Body;
                        context.Response.Body = responseBodyMemoryStream;

                        long start = Stopwatch.GetTimestamp();

                        await _next(context);

                        double elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                        context.Response.Body.Seek(0, SeekOrigin.Begin);

                        string responseBody;

                        using (StreamReader sr = new StreamReader(context.Response.Body))
                        {
                            responseBody = await sr.ReadToEndAsync();
                            context.Response.Body.Seek(0, SeekOrigin.Begin);

                            string endResponseBody = (responseBody.Length > maxLogFieldLength) ?
                                responseBody.Substring(0, maxLogFieldLength) : responseBody;

                            Log.ForContext(YaLogKeys.ResponseHeaders, context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true)
                                .ForContext(YaLogKeys.StatusCode, context.Response.StatusCode)
                                .ForContext(YaLogKeys.ResponseBody, endResponseBody)
                                .ForContext(YaLogKeys.ElapsedMilliseconds, elapsedMs)
                                .ForContext(YaLogKeys.RequestProtocol, context.Request.Protocol)
                                .ForContext(YaLogKeys.RequestScheme, context.Request.Scheme)
                                .ForContext(YaLogKeys.RequestHost, context.Request.Host.Value)
                                .ForContext(YaLogKeys.RequestMethod, context.Request.Method)
                                .ForContext(YaLogKeys.RequestPath, context.Request.Path)
                                .ForContext(YaLogKeys.RequestQuery, context.Request.QueryString)
                                .ForContext(YaLogKeys.RequestPathAndQuery, GetFullPath(context))
                                .ForContext(YaLogKeys.RequestAborted, context.RequestAborted.IsCancellationRequested)
                                .Information("HTTP request handled.");

                            await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference, lifetime.ApplicationStopping);
                        }
                    }
                }
            }
        }

        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        private static string GetFullPath(HttpContext httpContext)
        {
            /*
                In some cases, like when running integration tests with WebApplicationFactory<T>
                the RawTarget returns an empty string instead of null, in that case we can't use
                ?? as fallback.
            */
            string requestPath = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = httpContext.Request.Path.ToString();
            }

            return requestPath;
        }
    }
}
