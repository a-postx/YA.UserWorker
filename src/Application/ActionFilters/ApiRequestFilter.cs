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
    /// Idempotency filter: saves request and result to return the same result in case of a duplicate request.
    /// </summary>
    public sealed class ApiRequestFilter : ActionFilterAttribute
    {
        public ApiRequestFilter(IApiRequestTracker apiRequestTracker, IRuntimeContextAccessor runtimeContextAccessor)
        {
            _runtimeContext = runtimeContextAccessor ?? throw new ArgumentNullException(nameof(runtimeContextAccessor));
            _apiRequestTracker = apiRequestTracker ?? throw new ArgumentNullException(nameof(apiRequestTracker));
        }

        private readonly IRuntimeContextAccessor _runtimeContext;
        private readonly IApiRequestTracker _apiRequestTracker;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string method = context.HttpContext.Request.Method;

            Guid correlationId = _runtimeContext.GetCorrelationId();

            if (correlationId != Guid.Empty)
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(Timeouts.ApiRequestFiterMs))
                {
                    (bool requestCreated, ApiRequest request) = await _apiRequestTracker.GetOrCreateRequestAsync(correlationId, method, cts.Token);

                    if (!requestCreated)
                    {
                        ApiProblemDetails apiError = new ApiProblemDetails("https://tools.ietf.org/html/rfc7231#section-6.5.8", StatusCodes.Status409Conflict,
                                context.HttpContext.Request.HttpContext.Request.Path.Value, "Api call is already exist.", null, request.ApiRequestID.ToString(),
                                context.HttpContext.Request.HttpContext.TraceIdentifier);

                        context.Result = new ConflictObjectResult(apiError);
                        return;
                    }
                }
            }
            else
            {
                context.Result = new BadRequestResult();
                return;
            }

            if (_runtimeContext.GetTenantId() == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next.Invoke();
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            string method = context.HttpContext.Request.Method;
            Guid correlationId = _runtimeContext.GetCorrelationId();

            if (correlationId != Guid.Empty)
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(Timeouts.ApiRequestFiterMs))
                {
                    (bool requestCreated, ApiRequest request) = await _apiRequestTracker.GetOrCreateRequestAsync(correlationId, method, cts.Token);

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
                                    ApiRequestResult apiRequestResult = new ApiRequestResult
                                    {
                                        StatusCode = objectRequestResult.StatusCode,
                                        Body = JToken.Parse(JsonConvert.SerializeObject(objectRequestResult.Value)).ToString(Formatting.None)
                                    };

                                    await _apiRequestTracker.SetResultAsync(request, apiRequestResult, cts.Token);
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
            }

            await next.Invoke();
        }
    }
}
