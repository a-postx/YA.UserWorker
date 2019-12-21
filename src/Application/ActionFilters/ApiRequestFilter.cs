using Delobytes.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Enums;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.ActionFilters
{
    /// <summary>
    /// Idempotency filter: saves request and result to return the same result in case of duplicate request.
    /// </summary>
    public sealed class ApiRequestFilter : ActionFilterAttribute
    {
        public ApiRequestFilter(IApiRequestTracker apiRequestTracker)
        {
            _apiRequestTracker = apiRequestTracker ?? throw new ArgumentNullException(nameof(apiRequestTracker));
        }

        private readonly IApiRequestTracker _apiRequestTracker;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                string method = context.HttpContext.Request.Method;
                IHeaderDictionary headers = context.HttpContext.Request.Headers;

                (bool requestCreated, ApiRequest request) = await GetOrCreateRequestAsync(headers, method, cts.Token);

                if (request == null)
                {
                    context.Result = new BadRequestResult();
                    return;
                }

                if (!requestCreated)
                {
                    ApiProblemDetails apiError = new ApiProblemDetails("https://tools.ietf.org/html/rfc7231#section-6.5.8", StatusCodes.Status409Conflict,
                        context.HttpContext.Request.HttpContext.Request.Path.Value, "Api call is already exist.", null, request.ApiRequestID.ToString(),
                        context.HttpContext.Request.HttpContext.TraceIdentifier);

                    context.Result = new ConflictObjectResult(apiError);
                    return;
                }
            }

            await next.Invoke();
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                string method = context.HttpContext.Request.Method;
                IHeaderDictionary headers = context.HttpContext.Request.Headers;

                (bool requestCreated, ApiRequest request) = await GetOrCreateRequestAsync(headers, method, cts.Token);

                if (request != null)
                {
                    switch (context.Result)
                    {
                        case ObjectResult objectRequestResult when objectRequestResult.Value is ApiProblemDetails apiError:
                            ////if (apiError.Code == ApiErrorCodes.DUPLICATE_API_CALL)
                            ////{
                            ////    if (request.ResponseBody != null)
                            ////    {
                            ////        try
                            ////        {
                            ////            JToken token = JToken.Parse(request.ResponseBody);
                            ////            JObject json = JObject.Parse((string)token);

                            ////            ObjectResult previousResult = new ObjectResult(json)
                            ////            {
                            ////                StatusCode = request.ResponseStatusCode
                            ////            };

                            ////            context.Result = previousResult;
                            ////        }
                            ////        catch (JsonReaderException)
                            ////        {
                            ////            //ignore object parsing exception as we return ApiError object in this case
                            ////        }
                            ////    }
                            ////}
                            break;
                        case ObjectResult objectRequestResult:
                        {
                            ApiRequestResult problemResult = new ApiRequestResult
                            {
                                StatusCode = objectRequestResult.StatusCode,
                                Body = JToken.Parse(JsonConvert.SerializeObject(objectRequestResult.Value)).ToString(Formatting.None)
                            };

                            await _apiRequestTracker.SetResultAsync(request, problemResult, cts.Token);
                            break;
                        }
                        case OkResult okResult:
                        {
                            ApiRequestResult result = new ApiRequestResult
                            {
                                StatusCode = okResult.StatusCode
                            };

                            await _apiRequestTracker.SetResultAsync(request, result, cts.Token);
                            break;
                        }
                    }
                }
            }

            await next.Invoke();
        }

        private async Task<(bool requestCreated, ApiRequest request)> GetOrCreateRequestAsync(IHeaderDictionary headers, string method, CancellationToken cancellationToken)
        {
            Guid correlationId = headers.GetCorrelationId(General.CorrelationIdHeader);

            return (correlationId == Guid.Empty) ? (false, null) : await _apiRequestTracker
                .GetOrCreateRequestAsync(correlationId, method, cancellationToken);
        }
    }
}
