using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Service;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Options;

namespace YA.TenantWorker.Application.Middlewares.ResourceFilters
{
    /// <summary>
    /// Фильтр идемпотентности: не допускает запросов без идентификатора,
    /// сохраняет запрос и результат в кеш, чтобы вернуть тот же ответ в случае запроса-дубликата.
    /// Реализация по примеру https://stripe.com/docs/api/idempotent_requests
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IdempotencyFilterAttribute : Attribute, IAsyncResourceFilter
    {
        public IdempotencyFilterAttribute(ILogger<IdempotencyFilterAttribute> logger,
            IApiRequestDistributedCache cacheService,
            IRuntimeContextAccessor runtimeContextAccessor,
            IOptions<GeneralOptions> options,
            IProblemDetailsFactory problemDetailsFactory)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _runtimeCtx = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _generalOptions = options.Value;
            _pdFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        }

        private readonly ILogger<IdempotencyFilterAttribute> _log;
        private readonly IApiRequestDistributedCache _cacheService;
        private readonly IRuntimeContextAccessor _runtimeCtx;
        private readonly GeneralOptions _generalOptions;
        private readonly IProblemDetailsFactory _pdFactory;

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (_generalOptions.IdempotencyFilterEnabled.HasValue && _generalOptions.IdempotencyFilterEnabled.Value)
            {
                Guid tenantId = _runtimeCtx.GetTenantId();
                Guid requestId = _runtimeCtx.GetClientRequestId();

                if (requestId == Guid.Empty)
                {
                    ProblemDetails problemDetails = _pdFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status400BadRequest,
                                $"Запрос не содержит заголовка {_generalOptions.ClientRequestIdHeader} или значение в нём неверно.",
                                null, null, context.HttpContext.Request.Path);

                    context.Result = new BadRequestObjectResult(problemDetails);
                    return;
                }

                string method = context.HttpContext.Request.Method;
                string path = context.HttpContext.Request.Path.HasValue ? context.HttpContext.Request.Path.Value : null;
                string query = context.HttpContext.Request.QueryString.HasValue ? context.HttpContext.Request.QueryString.ToUriComponent() : null;

                using (CancellationTokenSource cts = new CancellationTokenSource(Timeouts.ApiRequestFilterMs))
                {
                    (bool requestCreated, ApiRequest request) = await CheckAndGetOrCreateRequestAsync(tenantId, requestId, method, path, query);

                    if (!requestCreated)
                    {
                        if (method != request.Method || path != request.Path || query != request.Query)
                        {
                            ProblemDetails apiRequestParamError = _pdFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status409Conflict,
                            "В кеше исполнения уже есть запрос с таким идентификатором и его параметры отличны от текущего запроса.", null, null, context.HttpContext.Request.Path);

                            context.Result = new BadRequestObjectResult(apiRequestParamError);
                            return;
                        }

                        //доделать при необходимости: сразу возвращаем кешированный результат предыдущего запроса
                        //context.HttpContext.Response.StatusCode = request.StatusCode ?? 0;
                        //context.HttpContext.Response.Headers = request.Headers;

                        //if (request.StatusCode == StatusCodes.Status200OK)
                        //{
                        //    context.Result = new OkObjectResult(request.Body);
                        //}

                        //return;

                        ProblemDetails apiRequestConcurrencyError = _pdFactory
                            .CreateProblemDetails(context.HttpContext, StatusCodes.Status409Conflict,
                            "Запрос уже существует.", null, null, context.HttpContext.Request.Path);

                        context.Result = new ConflictObjectResult(apiRequestConcurrencyError);
                        return;
                    }

                    ResourceExecutedContext executedContext = await next.Invoke();

                    int statusCode = context.HttpContext.Response.StatusCode;
                    request.SetStatusCode(statusCode);
                    Dictionary<string, List<string>> headers = context
                        .HttpContext.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToList());
                    request.SetHeaders(headers);

                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new JsonStringEnumConverter());
                    options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                    options.WriteIndented = false;

                    if (executedContext.Result != null)
                    {
                        request.SetResultType(executedContext.Result.GetType().AssemblyQualifiedName);

                        switch (executedContext.Result)
                        {
                            case CreatedAtRouteResult createdRequestResult:
                            {
                                string body = JsonSerializer.Serialize(createdRequestResult.Value, options);
                                request.SetBody(body);

                                request.SetResultRouteName(createdRequestResult.RouteName);

                                Dictionary<string, string> routeValues = createdRequestResult
                                    .RouteValues.ToDictionary(r => r.Key, r => r.Value.ToString());
                                request.SetResultRouteValues(routeValues);

                                break;
                            }
                            case ObjectResult objectRequestResult:
                            {
                                string body = JsonSerializer.Serialize(objectRequestResult.Value, options);
                                request.SetBody(body);

                                break;
                            }
                            case NoContentResult noContentResult:
                            {
                                break;
                            }
                            case OkResult okResult:
                            {
                                break;
                            }
                            case StatusCodeResult statusCodeResult:
                            case ActionResult actionResult:
                            {
                                // Known types that do not need additional data
                                break;
                            }
                            default:
                            {
                                throw new NotImplementedException($"Idempotency handling is not implement for IActionResult type {executedContext.GetType()}");
                            }
                        }
                    }

                    await SetResponseAsync(request);
                }
            }
            else
            {
                await next.Invoke();
            }
        }

        private async Task<(bool created, ApiRequest request)> CheckAndGetOrCreateRequestAsync(Guid tenantId, Guid clientRequestId, string method, string path, string query)
        {
            string key = $"{tenantId}:idempotency_keys:{clientRequestId}";

            bool apiRequestIsCached = await _cacheService.ApiRequestExist(key);

            if (apiRequestIsCached)
            {
                ApiRequest requestFromCache = await _cacheService.GetApiRequestAsync(key);

                return (false, requestFromCache);
            }
            else
            {
                ApiRequest apiRequest = new ApiRequest(tenantId, clientRequestId);

                apiRequest.SetMethod(method);
                apiRequest.SetPath(path);
                apiRequest.SetQuery(query);

                await _cacheService.CreateApiRequestAsync(apiRequest);

                return (true, apiRequest);
            }
        }

        private async Task SetResponseAsync(ApiRequest request)
        {
            await _cacheService.UpdateApiRequestAsync(request);
        }
    }
}
