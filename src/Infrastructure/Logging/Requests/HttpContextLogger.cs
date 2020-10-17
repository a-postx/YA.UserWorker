using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using YA.Common.Constants;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Infrastructure.Logging.Requests
{
    /// <summary>
    /// Прослойка логирования HTTP-контекста - запросов, ответов и исключений. 
    /// </summary>
    public class HttpContextLogger
    {
        public HttpContextLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext httpContext,
            IHostEnvironment env,
            IHostApplicationLifetime lifetime,
            IRuntimeContextAccessor runtimeCtx,
            IOptions<GeneralOptions> options)
        {
            HttpContext context = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            int maxLogFieldLength = options.Value.MaxLogFieldLength;

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
                    .Information("{RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);

                using (MemoryStream responseBodyMemoryStream = new MemoryStream())
                {
                    Stream originalResponseBodyReference = context.Response.Body;
                    context.Response.Body = responseBodyMemoryStream;

                    long start = Stopwatch.GetTimestamp();

                    try
                    {
                        await _next(context);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;
                        Log.Error(ex.Demystify(), "{ErrorMessage}", errorMessage);

                        ProblemDetails unknownError = new ProblemDetails
                        {
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                            Status = StatusCodes.Status500InternalServerError,
                            Instance = context.Request.HttpContext.Request.Path,
                            Title = errorMessage,
                            Detail = env.IsDevelopment() ? ex.Demystify().StackTrace : null
                        };
                        unknownError.Extensions.Add("correlationId", runtimeCtx.GetCorrelationId().ToString());
                        unknownError.Extensions.Add("traceId", runtimeCtx.GetTraceId().ToString());

                        string errorResponseBody = JsonConvert.SerializeObject(unknownError);
                        context.Response.ContentType = "application/problem+json";
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                        await context.Response.WriteAsync(errorResponseBody, lifetime.ApplicationStopping);
                    }

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
                            .Information("{RequestMethod} {RequestPath} - {StatusCode} in {ElapsedMilliseconds} ms", context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsedMs);

                        await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference, lifetime.ApplicationStopping);
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
