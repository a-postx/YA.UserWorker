using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.Dto;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.ActionFilters
{
    /// <summary>
    /// Фильтр идемпотентности: не допускает запросов без корелляционного идентификатора
    /// и сохраняет запрос и результат чтобы вернуть тот же ответ в случае запроса-дубликата.
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
                using (CancellationTokenSource cts = new CancellationTokenSource(Timeouts.ApiRequestFilterMs))
                {
                    (bool requestCreated, ApiRequest request) = await _apiRequestTracker.GetOrCreateRequestAsync(correlationId, method, cts.Token);

                    if (!requestCreated)
                    {
                        ApiProblemDetails apiError = new ApiProblemDetails("https://tools.ietf.org/html/rfc7231#section-6.5.8", StatusCodes.Status409Conflict,
                                context.HttpContext.Request.HttpContext.Request.Path.Value, "Запрос уже существует.", null, request.ApiRequestID.ToString(),
                                context.HttpContext.Request.HttpContext.TraceIdentifier);

                        context.Result = new ConflictObjectResult(apiError);
                        return;
                    }
                }
            }
            else
            {
                ProblemDetails problemDetails = new ProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"Запрос не содержит заголовка {General.CorrelationIdHeader} или значение в нём неверно."
                };

                context.Result = new BadRequestObjectResult(problemDetails);
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
                using (CancellationTokenSource cts = new CancellationTokenSource(Timeouts.ApiRequestFilterMs))
                {
                    (bool requestCreated, ApiRequest request) = await _apiRequestTracker.GetOrCreateRequestAsync(correlationId, method, cts.Token);

                    if (request != null)
                    {
                        switch (context.Result)
                        {
                            case ObjectResult objectRequestResult when objectRequestResult.Value is ApiProblemDetails apiError:

                                break;
                            case ObjectResult objectRequestResult:
                                {
                                    string body = JToken.Parse(JsonConvert.SerializeObject(objectRequestResult.Value)).ToString(Formatting.None);
                                    ApiRequestResult apiRequestResult = new ApiRequestResult(objectRequestResult.StatusCode, body);

                                    await _apiRequestTracker.SetResultAsync(request, apiRequestResult, cts.Token);
                                    break;
                                }
                            case OkResult okResult:
                                {
                                    ApiRequestResult result = new ApiRequestResult(okResult.StatusCode, null);

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
