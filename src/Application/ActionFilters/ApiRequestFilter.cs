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
using YA.TenantWorker.Application.Models.ValueObjects;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.ActionFilters
{
    public sealed class ApiRequestFilter : ActionFilterAttribute
    {
        public ApiRequestFilter(IApiRequestManager apiRequestManager)
        {
            _apiRequestManager = apiRequestManager ?? throw new ArgumentNullException(nameof(apiRequestManager));
        }

        private readonly IApiRequestManager _apiRequestManager;

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
                    if (request != null)
                    {
                        ApiError requestExistsError = new ApiError(
                        ApiErrorTypes.Error,
                        ApiErrorCodes.DUPLICATE_API_CALL,
                        "Api call is already exist.",
                        request.ApiRequestID.ToString());

                        context.Result = new ConflictObjectResult(requestExistsError);
                        return;
                    }
                    else
                    {
                        context.Result = new BadRequestResult();
                        return;
                    }
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
                    if (context.Result is ObjectResult objectRequestResult)
                    {
                        if (objectRequestResult?.Value is ApiError apiError)
                        {
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
                        }
                        else
                        {
                            ApiRequestResult problemResult = new ApiRequestResult
                            {
                                StatusCode = objectRequestResult.StatusCode,
                                Body = JToken.Parse(JsonConvert.SerializeObject(objectRequestResult.Value)).ToString(Formatting.None)
                            };

                            await _apiRequestManager.SetResultAsync(request, problemResult, cts.Token);
                        }   
                    }
                    else
                    {
                        if (context.Result is OkResult okResult)
                        {
                            ApiRequestResult result = new ApiRequestResult
                            {
                                StatusCode = okResult.StatusCode
                            };

                            await _apiRequestManager.SetResultAsync(request, result, cts.Token);
                        }
                    }
                }
            }

            await next.Invoke();
        }

        private async Task<(bool requestCreated, ApiRequest request)> GetOrCreateRequestAsync(IHeaderDictionary headers, string method, CancellationToken cancellationToken)
        {
            Guid correlationId = headers.GetCorrelationId(General.CorrelationIdHeader);

            return (correlationId == Guid.Empty) ? (false, null) : await _apiRequestManager
                .GetOrCreateRequestAsync(correlationId, method, cancellationToken);
        }
    }
}
